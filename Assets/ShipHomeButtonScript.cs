using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipHomeButtonScript : MonoBehaviour
{
    [SerializeField] private int[] waterTilesIndices;
    public void CreateShip()
    {
        int randomTileIndex = GetRandomIndex();
        var offSceneState = new OffSceneShipSessionManager.OffSceneShipState
        {
            ShipID = "2",
            MovementStartTime = DateTime.UtcNow,
            TileIndex = randomTileIndex,
            ShipButtonIndex = 2,
        };
        OffSceneShipSessionManager.SetState(offSceneState.ShipID, offSceneState);
    }
    private int GetRandomIndex()
    {
        int randomTileIndex = waterTilesIndices[UnityEngine.Random.Range(0, waterTilesIndices.Length)];
        return randomTileIndex;
    }
}
