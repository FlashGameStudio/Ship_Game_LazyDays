using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[SelectionBase]
public class Unit : MonoBehaviour
{
    //public TheUnit unit;
    [SerializeField]
    private int movementPoints = 20;
    public int MovementPoints { get => movementPoints; }

    [SerializeField]
    private float movementDuration = 1f, rotationDuration = 0.01f;

    private GlowHighlight glowHighlight;
    protected Queue<Vector3> pathPositions = new Queue<Vector3>();
    protected Queue<Vector3> pathPositionsBack = new Queue<Vector3>();
    private List<Vector3> startingPath  = new List<Vector3>();

    public  Action<Unit> MovementFinished;
    public event Action<Unit, NPC, string, string> UnitEngagedNPC;

    public bool breakhunt = false;
    public Queue<Vector3> passedPath = new Queue<Vector3>();
    public List<Vector3Int> passedPathInt = new List<Vector3Int>();

    public float MovementDuration => movementDuration;

    public NPC target;

    public string ShipID { get; set; }

    private void Awake()
    {
        glowHighlight = GetComponent<GlowHighlight>();
    }

    public void Deselect()
    {
        //glowHighlight.ToggleGlow(false);
    }

    public void Select()
    {
        // glowHighlight.ToggleGlow();
        //Debug.Log("enter select method for unit.");
    }

    public static int GetTileIndexBasedOnTime(float time)
    {
        if (time < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(time), "Time must be non-negative.");
        }

        if (time >= 0 && time < 0.5)
        {
            return 1;
        }

        // Correct implementation based on the specified intervals.
        // For time between 0 to 0.5 seconds, it returns 1,
        // for 0.5 to 1.5 seconds, it returns 2, and so on.
        int index = (int)(time - 0.5f) + 2;
        //Debug.Log($"real movement time {time} tile index {index}");
        return index;
    }

    /// <summary>
    /// Exists to move ship itself
    /// </summary>
    /// <param name="currentPath"></param>
    public virtual void MoveThroughPath(List<Vector3> currentPath)
    {
        pathPositions = new Queue<Vector3>(currentPath);
        Vector3 firstTarget = pathPositions.Dequeue();
     
        StartCoroutine(RotationCoroutine(firstTarget, rotationDuration));
    }

    /// <summary>
    /// Method used to move unit through path towards target
    /// </summary>
    /// <param name="currentPath"></param>
    /// <param name="movementProgress"></param>
    public void MoveCaptainThroughPath(List<Vector3> currentPath, float movementProgress)
    {
        // Use this in coroutine - only for physical units on map
        startingPath = currentPath;
        pathPositions = new Queue<Vector3>();

        int currentProgress = GetTileIndexBasedOnTime(movementProgress);

        for (int x = currentProgress; x< currentPath.Count; x++)
            pathPositions.Enqueue(currentPath[x]);

        for (int x = 0; x <= currentProgress; x++)
            passedPath.Enqueue(currentPath[x]);

        //foreach(var a in pathPositions)
        //    Debug.Log($"{a.x},{a.y},{a.z}");

        Vector3 tile = pathPositions.Dequeue(); //currentPath[currentProgress];
        StartCoroutine(UnitRotationCoroutine(tile, rotationDuration, true));
    }

    /// <summary>
    /// Method used to move unit through path back from target (game manager)
    /// </summary>
    /// <param name="currentPath"></param>
    /// <param name="movementProgress"></param>
    public void MoveCaptainBackThroughPath(List<Vector3> currentPath, float movementProgress)
    {
        pathPositionsBack = new Queue<Vector3>();

        int currentProgress = GetTileIndexBasedOnTime(movementProgress);

        for (int x = currentProgress; x < currentPath.Count; x++)
            pathPositionsBack.Enqueue(currentPath[x]);

        Vector3 tile = pathPositionsBack.Dequeue();
        StartCoroutine(UnitRotationBackCoroutine(tile, rotationDuration));
    }

    /// <summary>
    /// Method used from Map Scene
    /// </summary>
    /// <param name="currentPath"></param>
    public void MoveCaptainBackThroughPath(List<Vector3> currentPath)
    {
        pathPositionsBack = new Queue<Vector3>(currentPath);
        if (pathPositionsBack.Count > 0)
        {
            Vector3 tile = pathPositionsBack.Dequeue();
            StartCoroutine(UnitRotationBackCoroutine(tile, rotationDuration));
        }
    }

    #region Player Movemevent Coroutines

    private IEnumerator RotationCoroutine(Vector3 endPosition, float rotationDuration)
    {
        Quaternion startRotation = transform.rotation;
        endPosition.y = transform.position.y;
        Vector3 direction = endPosition - transform.position;
        Quaternion endRotation = Quaternion.LookRotation(direction, Vector3.up);

        if (Mathf.Approximately(Mathf.Abs(Quaternion.Dot(startRotation, endRotation)), 1.0f) == false)
        {
            float timeElapsed = 0;
            while (timeElapsed < rotationDuration)
            {
                timeElapsed += Time.deltaTime;
                float lerpStep = timeElapsed / rotationDuration; // 0-1
                transform.rotation = Quaternion.Lerp(startRotation, endRotation, lerpStep);
                yield return null;
            }
            transform.rotation = endRotation;
            //GameManager.Instance.player.PlayerRotation = new Quaternion(endRotation.x, endRotation.y, endRotation.z, endRotation.w);//new GameObjects.SerializableQuaternion(endRotation.x, endRotation.y, endRotation.z, endRotation.w);
        }
        StartCoroutine(MovementCoroutine(endPosition));
    }

    private IEnumerator MovementCoroutine(Vector3 endPosition)
    {
        Vector3 startPosition = transform.position;
        endPosition.y = startPosition.y;
        float timeElapsed = 0;

        while (timeElapsed < movementDuration)
        {
            timeElapsed += Time.deltaTime;
            float lerpStep = timeElapsed / movementDuration;
            transform.position = Vector3.Lerp(startPosition, endPosition, lerpStep);
            yield return null;
        }
        transform.position = endPosition;

        if (pathPositions.Count > 0)
        {
           // Debug.Log("Selecting the next position!");
            StartCoroutine(RotationCoroutine(pathPositions.Dequeue(), rotationDuration));
        }
        else
        {
            //Debug.Log("Movement finished!");
            //GameManager.Instance.player.PlayerPosition = new Vector3(endPosition.x, endPosition.y, endPosition.z);//GameObjects.SerializableVector(endPosition.x, endPosition.y, endPosition.z);
            
            MovementFinished?.Invoke(this);
        }
    }

    #endregion

    #region Unit Movement Coroutines

    /// <summary>
    /// Rotation coroutine for a unit heading to the target, which is calling movement coroutine. endPosition is the position of the last tile in the list.
    /// </summary>
    /// <param name="endPosition"></param>
    /// <param name="rotationDuration"></param>
    /// <returns></returns>
    private IEnumerator UnitRotationCoroutine(Vector3 endPosition, float rotationDuration, bool first)
    {
        if (true)
        {
            Quaternion startRotation = transform.rotation;
            endPosition.y = transform.position.y;
            Vector3 direction = endPosition - transform.position;
            Quaternion endRotation = Quaternion.LookRotation(direction, Vector3.up);

            if (Mathf.Approximately(Mathf.Abs(Quaternion.Dot(startRotation, endRotation)), 1.0f) == false)
            {
                float timeElapsed = 0;
                while (timeElapsed < 0.5f * rotationDuration)
                {
                    if (breakhunt)
                    {
                        var newReversePath = new Queue<Vector3>(passedPath.Reverse());
                        passedPath = newReversePath;
                        //Destroy(this.gameObject);
                        StartCoroutine(UnitRotationBackBreakHuntCoroutine(passedPath.Dequeue()));
                    }

                    timeElapsed += Time.deltaTime;
                    float lerpStep = timeElapsed / 0.5f * rotationDuration; // 0-1
                    transform.rotation = Quaternion.Lerp(startRotation, endRotation, lerpStep);
                    yield return null;
                }
                transform.rotation = endRotation;
            }
        }
        StartCoroutine(UnitMovementCoroutine(endPosition));
    }

    /// <summary>
    /// Rotation coroutine for a unit heading back from target, which is calling movement coroutine. endPosition is the position of the last tile in the list.
    /// </summary>
    /// <param name="endPosition"></param>
    /// <param name="rotationDuration"></param>
    /// <returns></returns>
    private IEnumerator UnitRotationBackCoroutine(Vector3 endPosition, float rotationDuration)
    {
        Quaternion startRotation = transform.rotation;
        endPosition.y = transform.position.y;
        Vector3 direction = endPosition - transform.position;

        if (direction != Vector3.zero)
        {
            Quaternion endRotation = Quaternion.LookRotation(direction, Vector3.up);

            if (Mathf.Approximately(Mathf.Abs(Quaternion.Dot(startRotation, endRotation)), 1.0f) == false)
            {
                float timeElapsed = 0;
                while (timeElapsed < 0.5f * rotationDuration) // 2f*movementDuration for twice slower, 0.5f for twice faster
                {
                    timeElapsed += Time.deltaTime;
                    float lerpStep = timeElapsed / 0.5f * rotationDuration; // 2f*movementDuration for twice slower, 0.5f for twice faster
                    transform.rotation = Quaternion.Lerp(startRotation, endRotation, lerpStep);
                    yield return null;
                }
                transform.rotation = endRotation;
            }
        }
        StartCoroutine(UnitMovementBackCoroutine(endPosition));
    }

    protected virtual IEnumerator UnitMovementCoroutine(Vector3 endPosition)
    {
        Vector3 startPosition = transform.position;
        // Make sure y-axis is set to the height of the ground
        endPosition.y = startPosition.y;
        // Start timer
        float startTime = Time.time;
        // Go for 1 second (movement duration parameter)        
        // Break couroutine upon receiving break hunt parameter        
        while (Time.time - startTime < movementDuration) //2f*movementDuration for twice slower, 0.5f for twice faster
        {
            if (breakhunt)
            {
                //Debug.Log("BreakhuNt!");
                var newReversePath = new Queue<Vector3>(passedPath.Reverse());
                passedPath = newReversePath;
                //Destroy(this.gameObject);
                StartCoroutine(UnitRotationBackBreakHuntCoroutine(passedPath.Dequeue()));
                yield break;
            }                

            transform.position = Vector3.Lerp(startPosition, endPosition, (Time.time - startTime));

            yield return null;
        }
        // Adjust unit position to match target position
        transform.position = endPosition;

        if (target != null && endPosition == target.transform.position)
        {
            Debug.Log($"{DateTime.Now} [Client based] Reached NPC, triggered interaction");
            GameManager.Instance.unitReturning = true;
            //see if this can be optimized
            //StartCoroutine(FightEnemy(pathPositions.Dequeue(), rotationDuration));
            UnitEngagedNPC?.Invoke(this, target, this.gameObject.name, target.gameObject.name);
        }

        if (pathPositions.Count != 0)
        {
            //Debug.Log($"Time elapsed from movevement routite to rotation routine :{stopwatch.Elapsed}");
            // unit didnt pass all the tiles to the target continue with coroutines
            Vector3 tile = pathPositions.Dequeue();
            passedPath.Enqueue(tile);
            StartCoroutine(UnitRotationCoroutine(tile, rotationDuration, false));
        }
        else
        {
            // unit reached the target and triggers finish movement actions (see if this can be moved to the previous if/else
            Debug.Log($"{DateTime.Now} [Client based] Finished movement towards target");
            MovementFinished?.Invoke(this);
            UnitManager.instance.UnitReturnsFromCoords(this.gameObject.name, this.gameObject.transform.position, this.startingPath);
        }
    }

    private string GetPlayerIDFromCaptain(string captainName)
    {

        string playerID = string.Empty;
        playerID = captainName.Remove(0, 7).Replace("(Clone)", "");
        return playerID;
    }

    private IEnumerator UnitMovementBackCoroutine(Vector3 endPosition)
    {
        Vector3 startPosition = transform.position;
        // Make sure y-axis is set to the height of the ground
        endPosition.y = startPosition.y;
        // Start timer
        float timeElapsed = 0;
        // Go for 1 second (movement duration parameter)
        while (timeElapsed < movementDuration) //2f*movementDuration for twice slower, 0.5f for twice faster
        {            
            timeElapsed += Time.deltaTime;
            float lerpStep = timeElapsed / movementDuration; //2f*movementDuration for twice slower, 0.5f for twice faster
            transform.position = Vector3.Lerp(startPosition, endPosition, lerpStep);

            yield return null;
        }
        // Adjust unit position to match target position
        transform.position = endPosition;

        if (pathPositionsBack.Count != 0)
        {
            // unit didnt pass all the tiles to the target continue with coroutines
            StartCoroutine(UnitRotationBackCoroutine(pathPositionsBack.Dequeue(), rotationDuration));
        }
        else 
        {
            Debug.Log($"{DateTime.Now} [Client based] {transform.gameObject.name} finished movement back from target");
            MovementFinished?.Invoke(this);
            GameManager.Instance.huntInProgress = false;
            GameManager.Instance.unitReturning = false;
            //see if this can be optimized
            GameObject obj = GameObject.Find(transform.gameObject.name);
            Destroy(obj);
        }
    }

    private IEnumerator UnitRotationBackBreakHuntCoroutine(Vector3 endPosition)
    {
        Quaternion startRotation = transform.rotation;
        endPosition.y = transform.position.y;
        Vector3 direction = endPosition - transform.position;

        if (direction != Vector3.zero)
        {
            Quaternion endRotation = Quaternion.LookRotation(direction, Vector3.up);

            if (Mathf.Approximately(Mathf.Abs(Quaternion.Dot(startRotation, endRotation)), 1.0f) == false)
            {
                float timeElapsed = 0;
                while (timeElapsed < 0.5f * rotationDuration) // 2f*movementDuration for twice slower, 0.5f for twice faster
                {
                    timeElapsed += Time.deltaTime;
                    float lerpStep = timeElapsed / 0.5f * rotationDuration; // 2f*movementDuration for twice slower, 0.5f for twice faster
                    transform.rotation = Quaternion.Lerp(startRotation, endRotation, lerpStep);
                    yield return null;
                }
                transform.rotation = endRotation;
            }
        }
        StartCoroutine(UnitMovementBackBreakHuntCoroutine(endPosition));
    }

    private IEnumerator UnitMovementBackBreakHuntCoroutine(Vector3 endPosition)
    {
        Vector3 startPosition = transform.position;
        // Make sure y-axis is set to the height of the ground
        endPosition.y = startPosition.y;
        // Start timer
        float timeElapsed = 0;
        // Go for 1 second (movement duration parameter)
        while (timeElapsed < movementDuration) //2f*movementDuration for twice slower, 0.5f for twice faster
        {
            timeElapsed += Time.deltaTime;
            float lerpStep = timeElapsed / movementDuration; //2f*movementDuration for twice slower, 0.5f for twice faster
            transform.position = Vector3.Lerp(startPosition, endPosition, lerpStep);

            yield return null;
        }
        // Adjust unit position to match target position
        transform.position = endPosition;

        if (passedPath.Count != 0)
        {
            // unit didnt pass all the tiles to the target continue with coroutines
            StartCoroutine(UnitRotationBackBreakHuntCoroutine(passedPath.Dequeue()));
        }
        else
        {
            Debug.Log($"{DateTime.Now} [Client based] {transform.gameObject.name} finished movement back from target after break hunt");
            //Debug.Log($"{DateTime.Now} [Client based] Total players hunt events {GameManager.Instance.mapListOfHuntEvents.Count}");
            MovementFinished?.Invoke(this);
            GameManager.Instance.huntInProgress = false;
            GameManager.Instance.unitReturning = false; ;
            //see if this can be optimized
            GameObject obj = GameObject.Find(transform.gameObject.name);
            Destroy(obj);
        }
    }

    // ***********************TO DO***********************
    public IEnumerator FightEnemy()
    {
        //Debug.Log("NPC engaged");
        
        Animator animator = this.GetComponent<Animator>();
        if (animator != null)
            animator.Play("KayKit Animated Character|Attack(1h)"); //.SetBool("IsDefeated", true);

        yield return new WaitForSeconds(3); // 2 second delay       
    }

    public void PlayerDies()
    {
        StartCoroutine(EnemyPCSuccessfullRaidCoroutine());
    }

    private IEnumerator EnemyPCSuccessfullRaidCoroutine()
    {
        float timeElapsed = 0;

        while (timeElapsed < 0.31)
        {
            timeElapsed += Time.deltaTime;
            glowHighlight.ToggleGlow();
            yield return null;
        }

        glowHighlight.ToggleGlow(false);
    }
    #endregion
}
