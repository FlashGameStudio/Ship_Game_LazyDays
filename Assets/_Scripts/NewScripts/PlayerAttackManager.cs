using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerAttackManager : MonoBehaviour
{
    private const string FIRST_SHIP_ID = "1";
    [SerializeField] private HeroUnit heroPrefab;
    [SerializeField] private MovementSystem movementSystem;
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private PathLineRenderer pathLineRenderer;
    [SerializeField] private NPCRuntimeSetSO npcRuntimeSetSO;
    [SerializeField] private ShipRuntimeSetSO shipRuntimeSetSO;
    [SerializeField] private HexGrid hexGrid;

    [SerializeField] private int movementDuration = 2;
    [SerializeField] private int attackDuration = 5;

    private Transform ship1Transform;
    void OnEnable()
    {
        SetPlayerReferenceEvent.Instance += OnPlayerSpawned;
        SpawnHeroEvent.Instance += OnHeroSpawned;
        SpawnOffSceneHeroEvent.Instance += OnOffSceneHeroSpawned;
    }
    void OnDisable()
    {
        SetPlayerReferenceEvent.Instance -= OnPlayerSpawned;
        SpawnHeroEvent.Instance -= OnHeroSpawned;
        SpawnOffSceneHeroEvent.Instance -= OnOffSceneHeroSpawned;
    }
    private void OnPlayerSpawned(Transform player,string shipID)
    {
        if(shipID == FIRST_SHIP_ID)
        ship1Transform = player;
    }
    private void OnHeroSpawned(Transform spawnPoint, bool supportDestroyShip, string spawnFromShipID)
    {
        SpawnHeroAttacker(spawnPoint, supportDestroyShip, spawnFromShipID);
    }
    private void OnOffSceneHeroSpawned(OffSceneHeroData offSceneHeroData)
    {
        Transform spawnPoint = offSceneHeroData.shipTransform;
        var offSceneHero = SpawnHeroAttacker(spawnPoint, true, offSceneHeroData.SpawnFromShipID);
        offSceneHero.OverrideStartTimeForOffSceneHero(offSceneHeroData.MovementStartTime);
        offSceneHero.SaveSessionState();
        Destroy(offSceneHero.gameObject);
        OffSceneHeroRestoreEvent.Instance?.Invoke();
    }
    public void AttackEnemyUnit()
    {
        ActivateAttackDialog(false);
        if (shipRuntimeSetSO == null || shipRuntimeSetSO.Items.Count == 0) return;
        SpawnHeroAttacker(ship1Transform, false, "");
    }
    public void HideAttackDialog()
    {
        ActivateAttackDialog(false);
    }
    private HeroUnit SpawnHeroAttacker(Transform spawnPoint, bool supportDestroyShip, string spawnFromShipID)
    {
        var heroClone = Instantiate<HeroUnit>(heroPrefab, spawnPoint.position, spawnPoint.rotation, this.transform);
        HeroData heroData = new HeroData();
        heroData.CurrentPath = unitManager.CalculatePathForUnitToTarget(heroClone.transform.position, GetTargetNPC(heroClone.transform));
        heroData.ReversePath = PrepareReversePath(heroData, spawnPoint);
        heroData.LineRendereRecallPath = PrepareReverseRecallLineRendererPath(heroData, spawnPoint);
        heroData.HeroID = Guid.NewGuid().ToString();
        heroData.HeroClone = heroClone;
        heroData.ShipPosition = spawnPoint.position;
        heroData.hexGrid = hexGrid;
        heroData.MovementDuration = movementDuration;
        heroData.AttackDuration = attackDuration;
        heroData.ShipPosition = spawnPoint.position;
        heroData.CanDestroyShip = supportDestroyShip;
        heroData.SpawnedFromShipID = spawnFromShipID;

        heroClone.Init(heroData);
        pathLineRenderer.DrawPath(heroData);
        heroClone.MoveThroughPath(heroData.CurrentPath.Select(pos => hexGrid.GetTileAt(pos).transform.position).ToList());

        SetCameraTargetEvent.Instance?.Invoke(heroClone.transform, false);
        return heroClone;
    }
    private Vector3 GetTargetNPC(Transform hero)
    {
        if (unitManager.SelectedEnemy != null) return unitManager.SelectedEnemy.transform.position;

        float minDistance = float.MaxValue;
        NPC closestNPC = null;
        
        foreach (var npc in npcRuntimeSetSO.Items)
        {
            float distance = Vector3.Distance(npc.transform.position, hero.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestNPC = npc;
            }
        }
        return closestNPC.transform.position;
    }
    private List<Vector3Int> PrepareReversePath(HeroData heroData,Transform ship)
    {
        List<Vector3Int> reversePathGrid = new List<Vector3Int>(heroData.CurrentPath);
        if (reversePathGrid.Count > 0)
        {
            reversePathGrid.RemoveAt(reversePathGrid.Count - 1);
        }
        reversePathGrid.Reverse();
        Vector3Int playerPos = hexGrid.GetClosestHex(ship.position);
        reversePathGrid.Add(playerPos);

        return reversePathGrid;
    }
    private List<Vector3Int> PrepareReverseRecallLineRendererPath(HeroData heroData,Transform ship)
    {
        List<Vector3Int> reversePathGrid = new List<Vector3Int>(heroData.CurrentPath);
        reversePathGrid.Reverse();
        Vector3Int playerPos = hexGrid.GetClosestHex(ship.position);
        reversePathGrid.Add(playerPos);

        return reversePathGrid;
    }
   
    
    private void ActivateAttackDialog(bool value)
    {
        MapSceneInitializer.AttackDialog.SetActive(value);
    }
   
}
