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
        HeroID = trackerData.HeroID;
        currentTracerData = trackerData;
        TrackerHeroUnit = heroUnit;
        trackerTime = gameObject.AddComponent<Timer>();
        attackTimer = gameObject.AddComponent<Timer>();

        trackerTime.Duration = trackerData.TileCount * trackerData.MovementDuration;
        trackerTime.CoundDownText = countText;

        totalMovementDuration = trackerTime.Duration;

        attackTimer.Duration = totalMovementDuration + trackerData.AttackDuration; // count total time 

        attackTimer.Run();
        trackerTime.Run();

        attackTimer.TimerFinished += OnCountDownStarted;
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
}