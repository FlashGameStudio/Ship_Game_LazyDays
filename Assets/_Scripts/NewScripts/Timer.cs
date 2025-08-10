using UnityEngine;
using TMPro;
using System;

public class Timer : MonoBehaviour
{


    #region fields
    // Event support for Timer 
    public delegate void OnTimerFinished();
    public event OnTimerFinished TimerFinished;

    // timer Duration
    float totalSeconds=0;

    // timer Execution
    float elapsedSeconds=0;
    private bool running;

    // support for finished Property.
    private bool started;
   
    // support for remainingTime;
    float remainingSeconds;
    TMP_Text countDownText;

    #endregion

    #region Properties
    // used to set the duration of time.
    public float Duration
    {
        get
        {
            return totalSeconds;
        }
        set
        {
            if (!running)
                totalSeconds = value;
        }
    }

    // used to check whether Timer Finished.
    public bool Finished
    {
        get { return started && !running; }
    }

    // gets whether or not the timer is currently running
    public bool Running
    {
        get { return running; }
    }

    public TMP_Text CoundDownText
    {
        get { return countDownText; }
        set { countDownText = value; }
    }
    #endregion

    #region Methods
    // Update is called once per frame
    void Update()
    {
        // update timer and check for finished
        if (running)
        {
            elapsedSeconds += Time.unscaledDeltaTime;
            UpdateTextDown(countDownText);
            if (elapsedSeconds >= totalSeconds)
            {
                running = false;
                TimerFinished?.Invoke();
            }
        }
    }

    private void UpdateTextDown(TMP_Text text=null)
    {
        float remainingTime = Mathf.Ceil(totalSeconds - elapsedSeconds);
        TimeSpan time = TimeSpan.FromSeconds(remainingTime);
        text?.SetText(time.ToString(@"hh\:mm\:ss"));
    }
    public void ResetCounText(TMP_Text text)
    {
        text.SetText("00:00:00");
    }
    // Runs the timer
    // Because a timer of 0 duration doesn't really make sense,
    // the timer only runs if the total seconds is larger than 0
    // This also makes sure the consumer of the class has actually 
    // set the duration to something higher than 0
    public void Run()
    {
        if (totalSeconds > 0)
        {
            started = true;
            running = true;
            elapsedSeconds = 0;
        }
    }
    public void Stop()
    {
        started = false;
        running = false;
    }
    public void AddTime(float seconds)
    {
        remainingSeconds = totalSeconds - elapsedSeconds;
        remainingSeconds += seconds;
    }
    #endregion
}
