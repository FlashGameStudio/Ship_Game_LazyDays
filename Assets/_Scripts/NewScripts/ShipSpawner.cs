using System;
using UnityEngine;
using UnityEngine.UI;

public class ShipSpawner : MonoBehaviour
{
    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private Unit myUnit;
    [SerializeField] private Vector3 offset = new Vector3(0, 1, 0);
    [SerializeField] private Button[] shipButtons;
    [SerializeField] private int[] waterTilesIndices;
    [SerializeField] private ShipRuntimeSetSO shipRuntimeSetSO;

    private int firstShipIndex;


    void Start()
    {
        shipButtons[0]?.onClick.AddListener(() => SpawnShipOnButton("1", 0));
        shipButtons[1]?.onClick.AddListener(() => SpawnShipOnButton("2", 1, OnShipSpawned));
        ActivateShipButtonEvent.Instance += OnActivateShip2Button;
        RestoreShipSession();
        RestoreOffSceneShipSession();
    }
    void OnDestroy()
    {
        shipButtons[0]?.onClick.RemoveAllListeners();
        shipButtons[1]?.onClick.RemoveAllListeners();
        ActivateShipButtonEvent.Instance -= OnActivateShip2Button;
    }
    private void OnShipSpawned(Transform shipPosition, string spawnFromShipID)
    {
        SpawnHeroEvent.Instance?.Invoke(shipPosition, true, spawnFromShipID);
    }
    private void OnActivateShip2Button()
    {
        shipButtons[1].interactable = true;
    }
    private void RestoreOffSceneShipSession()
    {
        foreach (var state in OffSceneShipSessionManager.GetAllStates())
        {
            OffSceneShipSessionManager.OffSceneShipState shipState = state.Value;
            string shipID = state.Key;
            SpawnShipOffsceneSupport(shipState, OnOffSceneShipSpawned);
            OffSceneShipSessionManager.RemoveState(shipID);
        }
    }
    private void OnOffSceneShipSpawned(OffSceneHeroData offSceneData)
    {
        SpawnOffSceneHeroEvent.Instance?.Invoke(offSceneData);
    }
    private void RestoreShipSession()
    {
        foreach (var state in ShipSessionManager.GetAllStates())
        {
            ShipSessionManager.ShipState shipState = state.Value;
            string shipID = state.Key;
            ShipData data = new ShipData()
            {
                ShipID = shipID,
                SpawnPoint = shipState.SpawnPosition,
                shipButtonIndex = shipState.ShipButtonIndex,
            };
            if (data.shipButtonIndex < shipButtons.Length)
            {
                shipButtons[data.shipButtonIndex].interactable = false;
            }
            firstShipIndex = shipState.FirstShipIndex;
            var shipClone = SpawnShip(data);
            shipClone.ShipID = shipID;
            SetPlayerReferenceEvent.Instance?.Invoke(shipClone.transform, data.ShipID);
        }
    }
    public void SpawnShipOnButton(string shipID, int buttonIndex, Action<Transform, string> spawnHeroEvent = null)
    {
        int randomTileIndex = GetRandomIndex();
        Vector3 spawnPoint = hexGrid.waterTiles[randomTileIndex].position;
        Vector3 spawnPosition = spawnPoint + offset;

        ShipData data = new ShipData()
        {
            ShipID = shipID,
            SpawnPoint = spawnPosition,
            shipButtonIndex = buttonIndex,
        };
        var shipClone = SpawnShip(data);
        shipClone.ShipID = shipID;
        SaveShipState(data);

        spawnHeroEvent?.Invoke(shipClone.transform, shipID);

        shipButtons[buttonIndex].interactable = false;
        
        SetPlayerReferenceEvent.Instance?.Invoke(shipClone.transform, data.ShipID);
    }

    private int GetRandomIndex()
    {
        int randomTileIndex = waterTilesIndices[UnityEngine.Random.Range(0, waterTilesIndices.Length)];
        while (randomTileIndex == firstShipIndex)
        {
            randomTileIndex = waterTilesIndices[UnityEngine.Random.Range(0, waterTilesIndices.Length)];
        }
        firstShipIndex = randomTileIndex;
        return randomTileIndex;
    }
    private void SpawnShipOffsceneSupport(OffSceneShipSessionManager.OffSceneShipState state, Action<OffSceneHeroData> spawnHeroEvent)
    {
        Vector3 spawnPoint = hexGrid.waterTiles[state.TileIndex].position;
        Vector3 spawnPosition = spawnPoint + offset;
        ShipData data = new ShipData
        {
            ShipID = state.ShipID,
            SpawnPoint = spawnPosition,
            shipButtonIndex = state.ShipButtonIndex,
        };
        var shipClone = SpawnShip(data);
        shipClone.ShipID = state.ShipID;
        SaveShipState(data);

        OffSceneHeroData heroData = new OffSceneHeroData
        {
            MovementStartTime = state.MovementStartTime,
            SpawnFromShipID = state.ShipID,
            shipTransform = shipClone.transform
        };
        spawnHeroEvent?.Invoke(heroData);
        SetPlayerReferenceEvent.Instance?.Invoke(shipClone.transform, data.ShipID);
    }
    private Unit SpawnShip(ShipData shipData)
    {
        var ship = Instantiate<Unit>(myUnit, shipData.SpawnPoint, transform.rotation, transform);
        return ship;
    }
    private void SaveShipState(ShipData shipData)
    {
        ShipSessionManager.ShipState state = new ShipSessionManager.ShipState
        {
            ShipID = shipData.ShipID,
            SpawnPosition = shipData.SpawnPoint,
            ShipButtonIndex = shipData.shipButtonIndex,
            FirstShipIndex = firstShipIndex
        };
        ShipSessionManager.SetState(shipData.ShipID, state);
    }
}
public class ShipData
{
    public int shipButtonIndex;
    public Vector3 SpawnPoint;
    public string ShipID;
}
public class SetPlayerReferenceEvent
{
    public static Action<Transform,string> Instance;
}
public class SpawnHeroEvent
{
    public static Action<Transform, bool, string> Instance;
}
public class SpawnOffSceneHeroEvent
{
    public static Action<OffSceneHeroData> Instance;
}
public class ActivateShipButtonEvent
{
    public static Action Instance;
}
public class OffSceneHeroData
{
    public Transform shipTransform;
    public DateTime MovementStartTime;
    public string SpawnFromShipID;
}
