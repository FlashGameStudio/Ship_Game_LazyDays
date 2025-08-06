using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Unity.VisualScripting;
using UnityEngine;

public class HeroRestorer : MonoBehaviour
{
    private const string FIRST_SHIP_ID = "1";
    [SerializeField] private GameObject heroPrefab;
    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private Transform playerTransform;

    private bool hasStateRestored;
    private HashSet<string> uniquePathSignatures = new();
    void OnEnable()
    {
        SetPlayerReferenceEvent.Instance += OnPlayerSpawned;
        OffSceneHeroRestoreEvent.Instance += OnOffSceneHeroRestored;
    }
    void OnDisable()
    {
        SetPlayerReferenceEvent.Instance -= OnPlayerSpawned;
        OffSceneHeroRestoreEvent.Instance -= OnOffSceneHeroRestored;
    }
    private void OnPlayerSpawned(Transform player,string id)
    {
        if(id == FIRST_SHIP_ID) 
        playerTransform = player;

        if (!hasStateRestored)
        RestorePlayer();
    }
    private void OnOffSceneHeroRestored()
    {
        RestorePlayer();
    }
    private void RestorePlayer()
    {
        hasStateRestored = true;
        foreach (var kvp in HeroSessionManager.GetAllStates())
        {
            string heroId = kvp.Key;
            var state = kvp.Value;

            Vector3 spawnPos = state.StartPosition;
            spawnPos.y = 1f; // your intended Y height
            GameObject heroObj = Instantiate(heroPrefab, spawnPos, Quaternion.identity, this.transform);
            heroObj.name = heroId;

            HeroUnit heroUnit = heroObj.GetComponent<HeroUnit>();
            if (heroUnit == null)
                continue;

            // Rebuild a dummy HeroData or restore from somewhere
            HeroData data = new HeroData
            {
                HeroID = state.HeroID,
                hexGrid = hexGrid,
                CurrentPath = state.CurrentPath,
                ReversePath = state.ReversePath,
                PlayerTransform = playerTransform,
                HeroClone = heroUnit,
                MovementDuration = (int)state.MovementDurationPerTile,
                AttackDuration = (int)state.AttackDuration,
                HasRecalled = state.HasRecalled,
                LineRendereRecallPath = state.LineRendereRecallPath,
                ShipPosition = state.ShipPosition,
                CanDestroyShip = state.CanDestroyShip,
                SpawnedFromShipID = state.SpawnFromShipID
            };
            heroUnit.Init(data);
            heroUnit.RestoreFromSession(state);

            data.HasReturned = state.HasReturned; // updating variable here so that the session can update the state HasReturn value

            DrawUniquePath(data);
            SetCameraToTargetHero(heroUnit);
        }
    }

    private void DrawUniquePath(HeroData heroData)
    {
        if (heroData.HasReturned) return; // don't draw the path if player has reach to base while on off scene.

        if (heroData.CurrentPath != null && heroData.CurrentPath.Count > 0)
        {
            string pathSignature = $"{heroData.HeroID}-" +
            string.Join(",", heroData.CurrentPath.Select(p => $"{p.x}-{p.y}-{p.z}"));

            if (!uniquePathSignatures.Contains(pathSignature))
            {
                uniquePathSignatures.Add(pathSignature);

                DrawPathEvent.Instance?.Invoke(heroData);
            }
        }
    }

    private void SetCameraToTargetHero(HeroUnit heroUnit)
    {
        string followedHeroId = HeroSessionManager.GetFollowedHero();
        if (heroUnit.HeroId == followedHeroId)
        {
            SetCameraTargetEvent.Instance?.Invoke(heroUnit.transform, false);
        }
    }
}
