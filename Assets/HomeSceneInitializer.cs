using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomeSceneInitializer : MonoBehaviour
{
    [SerializeField]
    public GameObject eventsButton;

    [SerializeField]
    public GameObject huntButton;
    [SerializeField]
    public Image walkIcon;
    [SerializeField]
    public Image walkBackIcon;
    
    // Start is called before the first frame update

    void Start()
    {
        GameManager.Instance.currentScene = "HomeScene";

        if (!SceneManager.GetSceneByName("UI").IsValid())
            SceneManager.LoadScene("UI", LoadSceneMode.Additive);

        int currentTime = 0;// DTT.DailyRewards.UnixHelper.GetCurrentUnixTime();//

    }

        // Update is called once per frame
    void Update()
    {
        int currentTime = 0;
    }

    public static System.DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dateTime;
    }

    public void ShowEventsIcon(int endtime ,int currentTime)
    {
        eventsButton.SetActive(true);
        System.TimeSpan remaining = UnixTimeStampToDateTime(endtime) - UnixTimeStampToDateTime(currentTime);

        TMPro.TMP_Text eventText = eventsButton.GetComponentInChildren<TMPro.TMP_Text>();
        eventText.text = string.Format("{0:00}:{1:00}:{2:00}", remaining.Hours, remaining.Minutes, remaining.Seconds);
    }

    public void ShowMapMovementIcon(int endtime, int currentTime, bool walk, bool walkback)
    {
        huntButton.SetActive(true);
        System.TimeSpan remaining = UnixTimeStampToDateTime(endtime) - UnixTimeStampToDateTime(currentTime);

        TMPro.TMP_Text eventText = huntButton.GetComponentInChildren<TMPro.TMP_Text>();
        if (walk)
            walkIcon.gameObject.SetActive(true);
        else
            walkIcon.gameObject.SetActive(false);

        if (walkback)
            walkBackIcon.gameObject.SetActive(true);
        else
            walkBackIcon.gameObject.SetActive(false);

        //Image walk = huntButton.GetComponentInChildren
        eventText.text = string.Format("{0:00}:{1:00}:{2:00}", remaining.Hours, remaining.Minutes, remaining.Seconds);
    }

    public void ShowRemainingTime(int endtime, int currentTime)
    {
        //eventsButton.SetActive(true);
        System.TimeSpan remaining = UnixTimeStampToDateTime(endtime) - UnixTimeStampToDateTime(currentTime);

        TMPro.TMP_Text eventText = eventsButton.GetComponentInChildren<TMPro.TMP_Text>();
        eventText.text = string.Format("{0:00}:{1:00}:{2:00}", remaining.Hours, remaining.Minutes, remaining.Seconds);
    }

    public void HideEventsIcon()
    {
        eventsButton.SetActive(false);
    }

    public void HideMapMovementIcon()
    {
        huntButton.SetActive(false);
    }

    public void ShowMap()
    {
        //
            if (SceneManager.GetSceneByName("HomeScene").IsValid())
                SceneManager.UnloadSceneAsync("HomeScene");
            SceneManager.LoadScene("MapScene", LoadSceneMode.Additive);
       //
    }

    public void ShowDailyQuests()
    {
        string sourceScene = "HomeScene";
        string targetScene = "DailyScene";

        GameManager.Instance.callingScene = sourceScene;
        GameManager.Instance.RefreshScene(sourceScene, targetScene);
    }

    public void ShowInventory()
    {
        string sourceScene = "HomeScene";
        string targetScene = "InventoryScene";

        GameManager.Instance.callingScene = sourceScene;
        GameManager.Instance.RefreshScene(sourceScene, targetScene);
    }

    public void ShowHeroes()
    {
        string sourceScene = "HomeScene";
        string targetScene = "HeroesScene";

        GameManager.Instance.callingScene = sourceScene;
        GameManager.Instance.RefreshScene(sourceScene, targetScene);
    }

    public void ShowArmory()
    {
        string sourceScene = "HomeScene";
        string targetScene = "ArmoryScene";

        UISceneInitializer.UserInfo.SetActive(false);
        UISceneInitializer.Status.SetActive(false);
        UISceneInitializer.HMButton.SetActive(false);

        GameManager.Instance.callingScene = sourceScene;
        GameManager.Instance.RefreshScene(sourceScene, targetScene);
    }

    public void ShowTavern()
    {
        string sourceScene = "HomeScene";
        string targetScene = "TavernScene";

        UISceneInitializer.UserInfo.SetActive(false);
        UISceneInitializer.Status.SetActive(false);
        UISceneInitializer.HMButton.SetActive(false);

        GameManager.Instance.callingScene = sourceScene;
        GameManager.Instance.RefreshScene(sourceScene, targetScene);
    }

    public void ShowTrainingHall()
    {
        string sourceScene = "HomeScene";
        string targetScene = "TrainingHallScene";

        UISceneInitializer.UserInfo.SetActive(false);
        UISceneInitializer.Status.SetActive(false);
        UISceneInitializer.HMButton.SetActive(false);

        GameManager.Instance.callingScene = sourceScene;
        GameManager.Instance.RefreshScene(sourceScene, targetScene);
    }

    public void ShowRanking()
    {
        //Debug.Log("clicked on Training hall");
        string sourceScene = "HomeScene";
        string targetScene = "RankingScene";

        UISceneInitializer.UserInfo.SetActive(false);
        UISceneInitializer.Status.SetActive(false);
        UISceneInitializer.HMButton.SetActive(false);

        GameManager.Instance.callingScene = sourceScene;
        GameManager.Instance.RefreshScene(sourceScene, targetScene);
    }

    public void ShowShop()
    {
        //Debug.Log("clicked on Training hall");
        string sourceScene = "HomeScene";
        string targetScene = "ShopScene";

        UISceneInitializer.UserInfo.SetActive(false);
        UISceneInitializer.Status.SetActive(false);
        UISceneInitializer.HMButton.SetActive(false);

        GameManager.Instance.callingScene = sourceScene;
        GameManager.Instance.RefreshScene(sourceScene, targetScene);
    }
    public void ShowBank()
    {
        //Debug.Log("clicked on Training hall");
        string sourceScene = "HomeScene";
        string targetScene = "BankScene";

        UISceneInitializer.UserInfo.SetActive(false);
        UISceneInitializer.Status.SetActive(false);
        UISceneInitializer.HMButton.SetActive(false);
        UISceneInitializer.EventsButton.SetActive(false);

        GameManager.Instance.callingScene = sourceScene;
        GameManager.Instance.RefreshScene(sourceScene, targetScene);
    }
}
