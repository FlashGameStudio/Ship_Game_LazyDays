using System;
using System.Collections.Generic;
using UnityEngine;

public static class HeroSessionManager
{
    public class HeroState
    {
        public string HeroID;
        public Vector3 StartPosition;
        public List<Vector3> PathWorldPositions;
        public List<Vector3> ReversePathWorldPositions;
        public List<Vector3Int> CurrentPath;
        public List<Vector3Int> ReversePath;
        public List<Vector3Int> LineRendereRecallPath;
        public DateTime MovementStartTime;
        public float MovementDurationPerTile;
        public Vector3 ShipPosition;
        public int TotalPathTiles;
        public float AttackDuration;
        public bool IsReturning;
        public bool HasReturned;
        public bool HasRecalled;
        public bool CanDestroyShip;
        public string SpawnFromShipID;
    }

    private static Dictionary<string, HeroState> heroStates = new Dictionary<string, HeroState>();
    private static string followedHeroID;
    
    public static void SetFollowedHero(string heroID) => followedHeroID = heroID;
    public static string GetFollowedHero() => followedHeroID;
    public static void SetState(string heroId, HeroState state)
    {
        if (!heroStates.TryGetValue(heroId, out var existingState) ||
            state.MovementStartTime > existingState.MovementStartTime)
        {
            heroStates[heroId] = state;
        }
    }
    public static bool TryGetState(string heroId, out HeroState state)
    {
        return heroStates.TryGetValue(heroId, out state);
    }
    public static void RemoveState(string heroId)
    {
        if (heroStates.ContainsKey(heroId))
        {
            heroStates.Remove(heroId);
        }
    }

    public static Dictionary<string, HeroState> GetAllStates() => new(heroStates);

    public static void ClearAllStates()
    {
        heroStates.Clear();
    }

}
