using System;
using System.Collections.Generic;
using UnityEngine;

public class OffSceneShipSessionManager : MonoBehaviour
{
    public class OffSceneShipState
    {
        public string ShipID;
        public int TileIndex;
        public int ShipButtonIndex;
        public DateTime MovementStartTime;

    }
    private static Dictionary<string, OffSceneShipState> shipStates = new();

    public static Dictionary<string, OffSceneShipState> GetAllStates() => new(shipStates);

    public static void SetState(string shipID, OffSceneShipState state)
    {
        shipStates[shipID] = state;
    }
    public static bool TryGetState(string shipID, out OffSceneShipState state)
    {
        return shipStates.TryGetValue(shipID, out state);
    }
    public static void RemoveState(string shipID)
    {
        if (shipStates.ContainsKey(shipID))
        {
            shipStates.Remove(shipID);
        }
    }
}
public class OffSceneHeroRestoreEvent
{
    public static Action Instance;
}
