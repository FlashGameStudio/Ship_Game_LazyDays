using System.Collections.Generic;
using UnityEngine;

public static class ShipSessionManager
{
    public class ShipState
    {
        public string ShipID;
        public Vector3 SpawnPosition;
        public Quaternion SpawnRotation;
        public int ShipButtonIndex;
        public int FirstShipIndex;
    }
    private static Dictionary<string, ShipState> shipStates = new();

    public static Dictionary<string, ShipState> GetAllStates() => new(shipStates);

    public static void SetState(string shipID, ShipState state)
    {
        shipStates[shipID] = state;
    }
    public static bool TryGetState(string shipID, out ShipState state)
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

