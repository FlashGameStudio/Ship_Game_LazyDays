using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[SelectionBase]
public class NPC : MonoBehaviour
{
    public int movementPoints = 0;

    public int positionIndex;
    Vector3 position;
    public string authtoken;

    private GlowHighlight glowHighlight;
    private Queue<Vector3> pathPositions = new Queue<Vector3>();
    public GameObject npcGO;

    [SerializeField] NPCRuntimeSetSO runtimeSetSO;


    private void Start()
    {
        glowHighlight = GetComponent<GlowHighlight>();
        runtimeSetSO.AddItem(this);
    }
    private void OnDestroy()
    {
        runtimeSetSO.RemoveItem(this);
    }

    public void NPCIsEngaged()
    {
    }

    public void NPCPlayerDies(string player)
    {
        StartCoroutine(EnemyPCSuccessfullRaidCoroutine());
    }

    public void NPCIsEngagedWithDamageBackup(int damage)
    {
    }

    public void Deselect()
    {
    }

    public void Select()
    {
    }

    #region Lifecycle


    IEnumerator RemoveNPC(GameObject npcGO, string position)
    {
        yield return new WaitForSeconds(5); // 5 second delay

        Destroy(npcGO);
    }

    #endregion

    #region Vikings

    float movementDuration = 10;

    private IEnumerator NPCSinkingCoroutine()
    {
        yield return new WaitForSeconds(2);

        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition;
        endPosition.y = startPosition.y - 5;
        float timeElapsed = 0;

        while (timeElapsed < movementDuration)
        {
            timeElapsed += Time.deltaTime;
            float lerpStep = timeElapsed / movementDuration;
            transform.position = Vector3.Lerp(startPosition, endPosition, lerpStep);
            yield return null;
        }
        transform.position = endPosition;
        
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

    private IEnumerator ShipSinkingCoroutine()
    {
        yield return new WaitForSeconds(2);

        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation;
        endPosition.y = startPosition.y - 5;
        endRotation.x = startRotation.x + 2f;
        //endPosition.z = startPosition.x - 15;
        float timeElapsed = 0;

        while (timeElapsed < movementDuration)
        {
            timeElapsed += Time.deltaTime;
            float lerpStep = timeElapsed / movementDuration;
            transform.position = Vector3.Lerp(startPosition, endPosition, lerpStep);
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, lerpStep);
            yield return null;
        }
        transform.position = endPosition;
       
    }

    #endregion

    public void TakeDamage(int damage)
    {
    }
}
