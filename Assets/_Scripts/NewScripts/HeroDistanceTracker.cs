using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HeroDistanceTracker : MonoBehaviour
{

    private Timer trackerTime;
    [SerializeField] private TMP_Text countText;
    private Timer attackTimer;

    private float totalMovementDuration;

    public string HeroID { get; private set; }

    private TrackerData currentTracerData;

    public HeroUnit TrackerHeroUnit { get; set; }

    void OnEnable()
    {
        HeroRecalledTimeUpdateEvent.Instance += OnHeroRecallTimeUpdated;
    }
    void OnDisable()
    {
        HeroRecalledTimeUpdateEvent.Instance -= OnHeroRecallTimeUpdated;
    }

    public void SetTime(TrackerData trackerData, HeroUnit heroUnit)
    {
        CreateTimers();
        
        HeroID = trackerData.HeroID;
        currentTracerData = trackerData;
        TrackerHeroUnit = heroUnit;

        totalMovementDuration = trackerData.TileCount * trackerData.MovementDuration;
        trackerTime.Duration = totalMovementDuration;
        trackerTime.CoundDownText = countText;

        attackTimer.Duration = totalMovementDuration + trackerData.AttackDuration; // count total time 

        attackTimer.Run();
        trackerTime.Run();

        attackTimer.TimerFinished += OnCountDownStarted;
    }
    public void SetTimeForOffSceneHero(TrackerData trackerData, HeroUnit heroUnit)
    {
        ResetPreviousTimesTrackers();
        CreateTimers();

        HeroID = trackerData.HeroID;
        currentTracerData = trackerData;
        TrackerHeroUnit = heroUnit;
        

        totalMovementDuration = trackerData.TileCount * trackerData.MovementDuration;
        float totalTripDuration = totalMovementDuration + trackerData.AttackDuration;

        // How long ago hero started moving
        double elapsed = (DateTime.UtcNow - trackerData.MovementStartTime).TotalSeconds;

        trackerTime.CoundDownText = countText;

        if (elapsed < totalMovementDuration)
        {
            // Still moving towards attack
            trackerTime.Duration = Mathf.Max(0, totalMovementDuration - (float)elapsed);
            attackTimer.Duration = Mathf.Max(0, totalTripDuration - (float)elapsed);

            trackerTime.Run();
            attackTimer.Run();
            attackTimer.TimerFinished += OnCountDownStarted;
        }
        else if (elapsed < totalTripDuration)
        {
            // Currently in attack phase
            float timeInAttack = (float)elapsed - totalMovementDuration;
            float attackRemaining = Mathf.Max(0, trackerData.AttackDuration - timeInAttack);
            attackTimer.ResetCounText(countText);
            attackTimer.Duration = attackRemaining;
            attackTimer.Run();
            attackTimer.TimerFinished += OnCountDownStarted;
        }
        else
        {
            // Already returning to base
            float returnTripElapsed = (float)elapsed - totalTripDuration;
            float returnRemaining = Mathf.Max(0, totalMovementDuration - returnTripElapsed);

            trackerTime.Duration = returnRemaining;
            trackerTime.Run();
        }
    }

    private void CreateTimers()
    {
        if (trackerTime == null)
        {
            trackerTime = gameObject.AddComponent<Timer>();
        }
        if (attackTimer == null)
        {
            attackTimer = gameObject.AddComponent<Timer>();
        }
    }

    private void ResetPreviousTimesTrackers()
    {
        attackTimer.TimerFinished -= OnCountDownStarted;
        attackTimer.Stop();
        trackerTime.Stop();
    }
    public void OnCountDownStarted()
    {
        trackerTime.Duration = totalMovementDuration;
        trackerTime.Stop();
        attackTimer.Stop();
        trackerTime.Run();
        attackTimer.TimerFinished -= OnCountDownStarted;
    }
    private void OnHeroRecallTimeUpdated(HeroUnit heroUnit)
    {
        if (!heroUnit.HeroId.Equals(HeroID)) return;
        attackTimer.TimerFinished -= OnCountDownStarted;
        trackerTime.Stop();
        attackTimer.Stop();
        currentTracerData.TileCount = heroUnit.GetHeroData(heroUnit.HeroId).ReversePath.Count;
        trackerTime.Duration = currentTracerData.TileCount * currentTracerData.MovementDuration;
        trackerTime.Run();
    }
    public void ChangeCameraTarget()
    {
        HeroSessionManager.SetFollowedHero(HeroID);
        if (TrackerHeroUnit != null)
        {
            SetCameraTargetEvent.Instance?.Invoke(TrackerHeroUnit.transform, true); // true follow the target
        }
    }
}
public struct TrackerData
{
    public string HeroID;
    public int TileCount;
    public int MovementDuration;
    public int AttackDuration;
    public DateTime MovementStartTime;
}