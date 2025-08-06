using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Unity.VisualScripting;
public class UnitManager : MonoBehaviour
{
    private const string FIRST_SHIP_ID = "1";
    public bool dontDestroyOnLoad = true;

    // Work with
    public static UnitManager instance;

    [SerializeField]
    private Unit playerUnit;
    [SerializeField]
    public HexGrid hexGrid;

    [SerializeField]
    public MovementSystem movementSystem;

    public Vector3 PlayerPosition { get; set; }

    public bool PlayersTurn { get; private set; } = true;

    [SerializeField]
    private Unit selectedUnit;
    private Hex previouslySelectedHex;
    private NPC selectedNPC;

    public Transform SelectedEnemy { get; private set; }

    private void OnEnable()
    {
        SetPlayerReferenceEvent.Instance += OnPlayerSpawned;
    }
    private void OnDisable()
    {
        SetPlayerReferenceEvent.Instance -= OnPlayerSpawned;
    }
    private void OnPlayerSpawned(Transform player,string shipID)
    {
        if(shipID == FIRST_SHIP_ID)
        playerUnit = player.GetComponent<Unit>();
    }
    public void HandleUnitSelected(GameObject unit)
    {
        //Debug.Log("handle pc selected");
        if (PlayersTurn == false)
            return;

        Unit unitReference = unit.GetComponent<Unit>();

        //if (CheckIfTheSameUnitSelected(unitReference))
        //    return;

        PrepareUnitForMovement(unitReference);
    }

    public void HandleNPCSelected(GameObject npc)
    {
        MapSceneInitializer.PopupNPCInfo(npc);

        //Debug.Log("handle npc selected");

        //NPCSelected(npc);
       
    }
    public void HandleAttackOnNPCSelected(GameObject npc)
    {
        MapSceneInitializer.AttackOnNPCInfo(npc);
        SelectedEnemy = npc.transform;
    }
    public List<Vector3Int> CalculatePathForUnitToTarget(Unit unit, NPC npc)
    {
        Vector3Int npcPos = hexGrid.GetClosestHex(npc.transform.position);

        BFSResult movementRange = GraphSearch.BFSGetRangeForNPC(this.hexGrid, hexGrid.GetClosestHex(unit.transform.position));

        return movementRange.GetPathTo(hexGrid.GetTileAt(npcPos).HexCoords);
    }

    public List<Vector3Int> CalculatePathForUnitToTarget(Vector3 source, Vector3 destination)
    {
        Vector3Int npcPos = hexGrid.GetClosestHex(destination);

        BFSResult movementRange = GraphSearch.BFSGetRangeForNPC(hexGrid, hexGrid.GetClosestHex(source));

        return movementRange.GetPathTo(hexGrid.GetTileAt(npcPos).HexCoords);
    }


    public void UnitReturnsFromCoords(string playerCaptain, Vector3 startingPosition, List<Vector3> originalPath)
    {
        GameObject playergo = GameObject.Find(GetPlayerIDFromCaptain(playerCaptain));
        //Debug.Log($"captain {playerCaptain} ({startingPosition}) returning to {playergo.gameObject.name} ({playergo.transform.position})");
        List<Vector3> reversePath = originalPath;
        reversePath.Reverse();
        GameObject obj = GameObject.Find(playerCaptain);

        // Unit follows the path back
        movementSystem.MoveCaptainFromNPC(obj.GetComponent<Unit>(), this.hexGrid, GetPlayerIDFromCaptain(playerCaptain), reversePath);
    }


    public void UnitEngagesAnotherUnit(string playerID, string targetUnitToken, List<Vector3> curPath, Vector3 currentPosition, float movementProgress, bool goingTowards, bool returningFrom)
    {
        Unit cu = LoadCaptainIntoMap(playerID, currentPosition, curPath[curPath.Count-1]).GetComponent<Unit>();
        // attach events
        cu.MovementFinished += ResetCaptainsTurn;
        cu.UnitEngagedNPC += UnitEngagedNPC;
        // assign fields
        
        // Movement Progress only important when reentering map screen
        if (goingTowards)
            cu.MoveCaptainThroughPath(curPath, movementProgress);

        if (returningFrom)
            cu.MoveCaptainBackThroughPath(curPath, movementProgress);

    }


    private GameObject LoadCaptainIntoMap(GameObject playergo, Vector3 startPosition, Vector3 targetPosition)
    {
        //startPosition.y += 1;
        // Load unit 
        GameObject tempUnitGO = Resources.Load("Unit") as GameObject;
        tempUnitGO.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        // Instantiate a unit and let it go to the target
        GameObject tempmyGO = Instantiate(tempUnitGO, startPosition, Quaternion.identity);
        tempmyGO.gameObject.name = $"Captain{playergo.gameObject.name}";
        //tempmyGO.transform.LookAt(tempmyGO.transform.position + Vector3.forward);
        
        // Rotate captain towards target
        Vector3 relativePos = targetPosition - startPosition;
        // the second argument, upwards, defaults to Vector3.up
        Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
        tempmyGO.transform.rotation = rotation;
        
        //tempmyGO.transform.rotation = rotation;
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(tempmyGO, UnityEngine.SceneManagement.SceneManager.GetSceneByName("MapScene"));
        tempUnitGO.SetActive(true);

        return tempmyGO;
    }

    private GameObject LoadCaptainIntoMap(string playerId, Vector3 currentPosition, Vector3 targetPosition)
    {
        
        //startPosition.y += 1;
        // Load unit if not on map
        GameObject tempmyGO = GameObject.Find($"Captain{playerId}");

        if (tempmyGO == null)
        {
            GameObject tempUnitGO = Resources.Load("Unit") as GameObject;
            tempUnitGO.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            // Instantiate a unit and let it go to the target
            tempmyGO = Instantiate(tempUnitGO, currentPosition, Quaternion.identity);
            tempmyGO.gameObject.name = $"Captain{playerId}";
            //tempmyGO.transform.LookAt(tempmyGO.transform.position + Vector3.forward);

            // Rotate captain towards target
            Vector3 relativePos = targetPosition - currentPosition;
            // the second argument, upwards, defaults to Vector3.up
            Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
            tempmyGO.transform.rotation = rotation;

            tempUnitGO.SetActive(true);
        }
        //tempmyGO.transform.rotation = rotation;
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(tempmyGO, UnityEngine.SceneManagement.SceneManager.GetSceneByName("MapScene"));
        

        return tempmyGO;
    }
    public void HandleTerrainSelected(GameObject hexGO)
    {
        MapSceneInitializer.PopupTileInfo(hexGO);
        //Debug.Log("handle terrain selected");
        if (selectedUnit == null || PlayersTurn == false)
        {
            return;
        }

        Hex selectedHex = hexGO.GetComponent<Hex>();

        if (HandleHexOutOfRange(selectedHex.HexCoords) || HandleSelectedHexIsUnitHex(selectedHex.HexCoords))
            return;

        HandleTargetHexSelected(selectedHex);

    }

    private void PrepareUnitForMovement(Unit unitReference)
    {
        if (this.selectedUnit != null)
        {
            ClearOldSelection();
        }

        this.selectedUnit = unitReference;
        //this.selectedUnit.Select();
        //movementSystem.ShowRange(unitReference, this.hexGrid);
        movementSystem.ShowRange(this.selectedUnit, this.hexGrid);
    }

    private void CreateMovementRangeHexForNPC(NPC npcReference)
    {
        if (this.selectedNPC != null)
        {
            ClearNPCOldSelection();
        }

        this.selectedNPC = npcReference;
        movementSystem.PrepareRangeForNPC(this.selectedUnit, this.hexGrid);
    }

    private void CreateMovementRangeHexForNPC2(NPC npcReference, Unit gameunit, ref NPC selectedNPC)
    {
        if (selectedNPC != null)
        {
            ClearNPCOldSelection();
        }

        selectedNPC = npcReference;
        // this method sets movement range to all hexes
        movementSystem.PrepareRangeForNPC(gameunit, this.hexGrid);
    }

    private void ClearOldSelection()
    {
        previouslySelectedHex = null;
        this.selectedUnit.Deselect();
        movementSystem.HideRange(this.hexGrid);
        this.selectedUnit = null;

    }

    private void ClearNPCOldSelection()
    {
        previouslySelectedHex = null;
        //this.selectedNPC.Deselect();
        movementSystem.HideRange(this.hexGrid);
        this.selectedNPC = null;

    }

    private void HandleTargetHexSelected(Hex selectedHex)
    {
        if (previouslySelectedHex == null || previouslySelectedHex != selectedHex)
        {
            //Debug.Log("mouse clicked on hex first time and target hex selected");
            previouslySelectedHex = selectedHex;
            movementSystem.ShowPath(selectedHex.HexCoords, this.hexGrid);
        }
        else
        {
            //Debug.Log("mouse clicked on hex second time and unit starts movement to target");
            //link to spending energy while moving
            movementSystem.MoveUnit(selectedUnit, this.hexGrid);
            PlayersTurn = false;
            selectedUnit.MovementFinished += ResetTurn;
            ClearOldSelection();
        }
    }

    private bool HandleSelectedHexIsUnitHex(Vector3Int hexPosition)
    {
        if (hexPosition == hexGrid.GetClosestHex(selectedUnit.transform.position))
        {
            selectedUnit.Deselect();
            ClearOldSelection();
            return true;
        }
        return false;
    }

    private bool HandleHexOutOfRange(Vector3Int hexPosition)
    {
        if (movementSystem.IsHexInRange(hexPosition) == false)
        {
            //Debug.Log("Hex Out of range!");
            return true;
        }
        return false;
    }

    private void ResetTurn(Unit selectedUnit)
    {
        selectedUnit.MovementFinished -= ResetTurn;
        PlayersTurn = true;
        UpdateUnitStates(selectedUnit);
    }
    private void UpdateUnitStates(Unit selectedUnit)
    {
        foreach (var state in ShipSessionManager.GetAllStates())
        {
            ShipSessionManager.ShipState shipState = state.Value;
            if (state.Key == selectedUnit.ShipID)
            {
                shipState.SpawnPosition = selectedUnit.transform.position;
                ShipSessionManager.SetState(state.Key, shipState);
            }
        }
    }

    private void ResetCaptainsTurn(Unit selectedUnit)
    {
        ClearNPCOldSelection();
        selectedUnit.MovementFinished -= ResetCaptainsTurn;
        selectedUnit.passedPath.Clear();
        //PlayersTurn = true;
        string player = GetPlayerIDFromCaptain(selectedUnit.gameObject.name);
    }
    

    private string GetPlayerIDFromCaptain(string captainName)
    {

        string playerID = string.Empty;
        playerID = captainName.Remove(0, 7).Replace("(Clone)", "");
        return playerID;
    }

    public IEnumerator FightEnemy(Unit attackingUnit, NPC engagedNPC)
    {
        Debug.Log("fight animation coroutine");

        Animator animator = attackingUnit.GetComponent<Animator>();
        if (animator != null)
        {
            Debug.Log("animator exists");
            animator.Play("KayKit Animated Character|Attack(1h)"); //.SetBool("IsDefeated", true);
        }

        yield return new WaitForSeconds(3); // 2 second delay

        //NPCReturnFrom(engagedNPC);
    }

    private void UnitEngagedNPC(Unit attackingUnit, NPC engagedNPC, string player, string npcToken)
    {

        Debug.Log("Unit engaged NPC");
        attackingUnit.UnitEngagedNPC -= UnitEngagedNPC;
    }

    void Awake()
    {
        if (dontDestroyOnLoad)
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                //Debug.Log("instance of AM created");
            }
            else
            {
                DestroyImmediate(gameObject);
            }

            DontDestroyOnLoad(this);
        }
    }
}
