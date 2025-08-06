using System.Collections.Generic;
using UnityEngine;

public class TimeTrackerSpawner : MonoBehaviour
{
    [SerializeField] private HeroDistanceTracker distanceTrackerPrefab;
    [SerializeField] private Transform distanceTrackerParent;

    public Dictionary<string, HeroDistanceTracker> HeroTimeTrackers = new();
    private void OnEnable()
    {
        TimeTrackerSpawnEvent.Instance += OnTimeTrackerSpawned;
        HeroUnit.ReachToBaseEvent += OnHeroReachBase;
    }
    private void OnDisable()
    {
        TimeTrackerSpawnEvent.Instance -= OnTimeTrackerSpawned;
        HeroUnit.ReachToBaseEvent -= OnHeroReachBase;
    }

    private void OnTimeTrackerSpawned(TrackerData trackerData, HeroUnit heroUnit)
    {
        if (HeroTimeTrackers.TryGetValue(trackerData.HeroID, out var existingTracker))
        {
            if (trackerData.HeroID == heroUnit.HeroId)
            {
                existingTracker.TrackerHeroUnit = heroUnit;
            }
            return;
        }
        var trackerClone = Instantiate<HeroDistanceTracker>(distanceTrackerPrefab, transform.position, Quaternion.identity, distanceTrackerParent);
        trackerClone.SetTime(trackerData,heroUnit);
        HeroTimeTrackers[trackerClone.HeroID] = trackerClone;
    }
    private void OnHeroReachBase(HeroUnit hero)
    {
        string heroID = hero.HeroId;
        if (!string.IsNullOrEmpty(heroID) && HeroTimeTrackers.TryGetValue(heroID, out HeroDistanceTracker tracker))
        {
            Destroy(tracker.gameObject);
            HeroTimeTrackers.Remove(heroID);
        }
    }
} 
