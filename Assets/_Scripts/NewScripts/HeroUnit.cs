using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.SceneManagement;

public class HeroUnit : Unit
{
    private Timer attackTimer;
    private Coroutine movementCoroutine;
    private DateTime movementStartTime;

    public string HeroId { get; private set; }

    private Queue<Vector3> fullPathPositions;
    private Queue<Vector3> fullPathBackPositions;
    private bool isReturning;
    private int coveredPathCount = 0;
    public static event Action<HeroUnit> ReachToBaseEvent;
    public bool hasReachedToBase = false;

    [SerializeField] private ShipRuntimeSetSO shipRuntimeSetSO;

    public static List<HeroUnit> heroUnits = new List<HeroUnit>();

    private static Dictionary<string, HeroData> heroDataStates = new();
    private HexGrid hexGrid;
    public void Init(HeroData heroData)
    {
        HeroId = heroData.HeroID;
        SetHeroData(heroData);
        hexGrid = heroData.hexGrid;
        pathPositionsBack = new Queue<Vector3>(heroData.ReversePath.Select(pos => hexGrid.GetTileAt(pos).transform.position).ToList());
        fullPathPositions = new Queue<Vector3>(heroData.CurrentPath.Select(pos => hexGrid.GetTileAt(pos).transform.position).ToList());
        fullPathBackPositions = new Queue<Vector3>(pathPositionsBack);

        TrackerData data = GetUpdateTrackerData(heroData);
        TimeTrackerSpawnEvent.Instance?.Invoke(data, this);

        attackTimer = gameObject.AddComponent<Timer>();
        attackTimer.Duration = heroData.AttackDuration;
        MovementFinished += RunAttackTimer;
        attackTimer.TimerFinished += OnTimerFinished;
        RecallHeroUnitEvent.Instance += OnHeroRecalled;

        heroUnits.Add(heroData.HeroClone);
    }
    private void SetHeroData(HeroData heroData)
    {
        heroDataStates[HeroId] = heroData;
    }
    public HeroData GetHeroData(string id)
    {
        if (heroDataStates.TryGetValue(id, out HeroData data))
        {
            return data;
        }
        return new HeroData();
    }
    void OnDestroy()
    {
        MovementFinished -= RunAttackTimer;
        attackTimer.TimerFinished -= OnTimerFinished;
        RecallHeroUnitEvent.Instance -= OnHeroRecalled;
        heroUnits.Remove(this);
        DropCameraFromPlayer();
        SaveSessionState();  
    }
    private void DropCameraFromPlayer()
    {
        HeroUnit heroUnit = heroUnits.Count == 0 ? null : heroUnits[heroUnits.Count - 1];
        if (heroUnit != null)
        {
            SetCameraTargetEvent.Instance?.Invoke(heroUnit.transform, false); // assign new Hero to camera in order to follow it.
        }
        else
        {
            SetCameraTargetEvent.Instance?.Invoke(null, false); // true means get the tile (hex) instead of the player itself.
            ResetCameraFollowEvent.Instance?.Invoke();
        }
    }
    public override void MoveThroughPath(List<Vector3> currentPath)
    {
        if (movementCoroutine != null)
            StopCoroutine(movementCoroutine);
        movementStartTime = DateTime.UtcNow;
        pathPositions = new Queue<Vector3>(currentPath);
        movementCoroutine = StartCoroutine(MoveAlongRotation(pathPositions));
    }
    public void OverrideStartTimeForOffSceneHero(DateTime newMovementStartTime)
    {
        movementStartTime = newMovementStartTime;
    }

    private IEnumerator MoveAlongRotation(Queue<Vector3> pathPositions)
    {
        HeroData heroData = GetHeroData(HeroId);
        while (pathPositions.Count > 0)
        {
            Vector3 target = pathPositions.Dequeue();
            target.y = transform.position.y;

            Vector3 startPos = transform.position;
            Vector3 moveDir = (target - startPos);
            if (moveDir == Vector3.zero) continue; // no move needed

            moveDir.Normalize();
            Quaternion startRot = transform.rotation;
            Quaternion endRot = Quaternion.LookRotation(moveDir, Vector3.up);

            float elapsed = 0f;
            while (elapsed < heroData.MovementDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / heroData.MovementDuration);
                transform.position = Vector3.Lerp(startPos, target, t);
                transform.rotation = Quaternion.Slerp(startRot, endRot, t * 50);
                yield return null;
            }
            transform.position = target;
            transform.rotation = endRot;
            coveredPathCount++;
        }
        MovementFinished?.Invoke(this);
        if (isReturning)
        {
            ClearPathAndPlayerState();
            DestroyShip(heroData);
        }
    }

    private void DestroyShip(HeroData heroData)
    {
        if (!heroData.CanDestroyShip) return;
        foreach (var shipIdentifier in shipRuntimeSetSO.Items)
        {
            Unit ship = shipIdentifier.GetComponent<Unit>();
            if (string.IsNullOrEmpty(ship.ShipID) || !ship.ShipID.Equals(heroData.SpawnedFromShipID)) continue;
            Destroy(ship.gameObject);
            ShipSessionManager.RemoveState(ship.ShipID);
            ActivateShipButtonEvent.Instance?.Invoke();
            return;
        }
    }

    private TrackerData GetUpdateTrackerData(HeroData heroData)
    {
        TrackerData newTrackerData = new TrackerData
        {
            HeroID = heroData.HeroID,
            TileCount = heroData.CurrentPath.Count,
            MovementDuration = heroData.MovementDuration,
            AttackDuration = heroData.AttackDuration
        };
        return newTrackerData;
    }
    private void ClearPathAndPlayerState()
    {
        ReachToBaseEvent?.Invoke(this);
        Destroy(this.gameObject);
        HeroSessionManager.RemoveState(HeroId);
        RemoveHeroData(HeroId);
        hasReachedToBase = true; // player destroyed by reaching to base,now don't need to save the session for the player.
    }
    private void RemoveHeroData(string id)
    {
        if (heroDataStates.ContainsKey(id))
        {
            heroDataStates.Remove(id);
        }
    }
    private void RunAttackTimer(Unit unit)
    {
        attackTimer.Run();
    }

    private void OnTimerFinished()
    {
        attackTimer.Stop();

        MovementFinished -= RunAttackTimer;
        HeroData heroData = GetHeroData(HeroId);
        MoveThroughPath(heroData.ReversePath.Select(pos => hexGrid.GetTileAt(pos).transform.position).ToList());
        Debug.Log("Attack Finished");
        isReturning = true;
    }
    public void SaveSessionState()
    {
        if (hasReachedToBase) return;
        
        HeroData heroData = GetHeroData(HeroId);
        if (heroData == null || string.IsNullOrEmpty(heroData.HeroID)) return;
        
        var state = new HeroSessionManager.HeroState
        {
            HeroID = heroData.HeroID,
            StartPosition = transform.position,
            PathWorldPositions = fullPathPositions.ToList(),
            ReversePathWorldPositions = fullPathBackPositions.ToList(),
            CurrentPath = heroData.CurrentPath,
            ReversePath = heroData.ReversePath,
            LineRendereRecallPath = heroData.LineRendereRecallPath,
            MovementStartTime = movementStartTime, // new
            MovementDurationPerTile = heroData.MovementDuration,
            TotalPathTiles = heroData.CurrentPath.Count,
            AttackDuration = heroData.AttackDuration,
            IsReturning = isReturning,
            HasRecalled = heroData.HasRecalled,
            ShipPosition = heroData.ShipPosition,
            CanDestroyShip = heroData.CanDestroyShip,
            SpawnFromShipID = heroData.SpawnedFromShipID,
        };
        
        HeroSessionManager.SetState(HeroId, state);
    }
    public void RestoreFromSession(HeroSessionManager.HeroState state)
    {
        TimeSpan timePassed = DateTime.UtcNow - state.MovementStartTime;
        float totalSecondsPassed = (float)timePassed.TotalSeconds;

        float forwardDuration = state.PathWorldPositions.Count * state.MovementDurationPerTile;
        float attackDuration = state.AttackDuration;
        float returnDuration = forwardDuration;
        isReturning = state.IsReturning;

        if (!state.IsReturning && totalSecondsPassed < forwardDuration)
        {
            // CASE 1: Forward movement in progress
            ResumeForwardMovement(state, totalSecondsPassed);
        }
        else if (!state.IsReturning && totalSecondsPassed < forwardDuration + attackDuration)
        {
            // CASE 2: Attacking
            float timeAlreadyWaited = Mathf.Max(0f, totalSecondsPassed - forwardDuration);
            EnterAttackState(state, timeAlreadyWaited);
        }
        else if (!state.IsReturning)
        {
            // CASE 3: Finished attacking and starting return movement
            isReturning = true;
            attackTimer.TimerFinished -= OnTimerFinished;
            float timeSpentReturning = totalSecondsPassed - (forwardDuration + attackDuration);
            ResumeReturnMovement(state, timeSpentReturning);
            Debug.Log("Start Returning");
        }
        else
        {
            // Hero was already returning, and session resumed mid-return
            attackTimer.TimerFinished -= OnTimerFinished;
            float timeSpentReturning = totalSecondsPassed;
            ResumeReturnMovement(state, timeSpentReturning);
            Debug.Log("Return movement");

        }
    }


    private void ResumeForwardMovement(HeroSessionManager.HeroState state, float totalSecondsPassed)
    {
        Debug.Log("Forward movement");
        int tilesCrossed = Mathf.FloorToInt(totalSecondsPassed / state.MovementDurationPerTile);
        int maxTileIndex = state.PathWorldPositions.Count - 1;
        int pathIndex = Mathf.Clamp(tilesCrossed - 1, 0, maxTileIndex);

        Vector3 newPos = state.PathWorldPositions[pathIndex];
        newPos.y = 1;
        transform.position = newPos;

        var remainingPath = state.PathWorldPositions.Skip(pathIndex).ToList();
        if (remainingPath.Count > 0)
        {
            pathPositions = new Queue<Vector3>(remainingPath);
            movementCoroutine = StartCoroutine(MoveAlongRotation(pathPositions));
        }
        else
        {
            MovementFinished?.Invoke(this);
        }
    }
    private void EnterAttackState(HeroSessionManager.HeroState state, float timeAlreadyWaited)
    {
        Debug.Log("Enter Attack State");
        Vector3 posAtLastTile = state.PathWorldPositions[^1];
        posAtLastTile.y = 1f;
        transform.position = posAtLastTile;
        float newAttackDuration = state.AttackDuration - timeAlreadyWaited;
        attackTimer.Duration = newAttackDuration;
        attackTimer.Run();
    }
    private void ResumeReturnMovement(HeroSessionManager.HeroState state, float timeAlreadyReturning)
    {
        var reversedPath = state.ReversePathWorldPositions.ToList();

        int tilesCrossed = Mathf.FloorToInt(timeAlreadyReturning / state.MovementDurationPerTile);
        int maxTileIndex = reversedPath.Count - 1;
        int pathIndex = Mathf.Clamp(tilesCrossed - 1, 0, maxTileIndex);

        Vector3 newPos = reversedPath[pathIndex];
        newPos.y = 1;
        transform.position = newPos;

        var remainingPath = reversedPath.Skip(pathIndex).ToList();
        if (remainingPath.Count > 1)
        {
            pathPositionsBack = new Queue<Vector3>(remainingPath);
            movementCoroutine = StartCoroutine(MoveAlongRotation(pathPositionsBack));
        }
        else
        {
            MovementFinished?.Invoke(this);
            state.HasReturned = true;
            HeroData heroData = GetHeroData(state.HeroID);
            DestroyShip(heroData);
            ClearPathAndPlayerState();
        }
    }

    public void OnHeroRecalled(HeroUnit heroUnit)
    {
        if (!heroUnit.HeroId.Equals(HeroId) || isReturning) return;

        HeroData heroData = GetHeroData(HeroId);

        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }
        attackTimer.TimerFinished -= OnTimerFinished;
        isReturning = true;
        heroData.HasRecalled = true;
        movementStartTime = DateTime.UtcNow; // set return start time

        pathPositions.Clear();
        fullPathPositions = new Queue<Vector3>();

        // Trim the reverse path to avoid rewalking already covered tiles
        TrimReversePathForRecall(heroData);

        // Start moving back
        movementCoroutine = StartCoroutine(MoveAlongRotation(pathPositionsBack));

        HeroRecalledTimeUpdateEvent.Instance?.Invoke(this);
        
        HeroRecalledPathUpdateEvent.Instance?.Invoke(heroData);
    }
    private void TrimReversePathForRecall(HeroData heroData)
    {

        int tilesToTrim = heroData.CurrentPath.Count - (coveredPathCount + 1);
        if (tilesToTrim.Equals(0)) return;
        // Skip trimmed tiles and rebuild the back path
        pathPositionsBack = new Queue<Vector3>(fullPathBackPositions.Skip(tilesToTrim));
        fullPathBackPositions = new Queue<Vector3>(pathPositionsBack);

        heroData.LineRendereRecallPath = heroData.LineRendereRecallPath.Skip(tilesToTrim).ToList();
        // Update ReversePath to reflect the trimmed one
        heroData.ReversePath = heroData.ReversePath.Skip(tilesToTrim).ToList();
       
    }
}
public class HeroData
{
    public string HeroID;
    public List<Vector3Int> CurrentPath;
    public List<Vector3Int> ReversePath;
    public List<Vector3Int> LineRendereRecallPath;
    public Transform PlayerTransform;
    public HeroUnit HeroClone;
    public int MovementDuration;
    public int AttackDuration;
    public bool HasReturned;
    public bool HasRecalled;
    public Vector3 ShipPosition;
    public HexGrid hexGrid;
    public bool CanDestroyShip;
    public string SpawnedFromShipID;
}
