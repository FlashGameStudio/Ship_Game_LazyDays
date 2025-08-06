using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScriptBottomMenu : MonoBehaviour
{
    [SerializeField]
    public GameObject newsNotify;
    [SerializeField]
    public TMP_Text newsCount;

    public static GameObject NewsNotify { get; set; }
    public static TMP_Text NewsCount { get; set; }  

    // Start is called before the first frame update
    void Start()
    {
        NewsNotify = newsNotify;
        NewsCount = newsCount;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public static void ActivateNews()
    {
        NewsNotify.SetActive(true);
        //NewsCount.text = GameManager.Instance.player.UnreadNewsCount.ToString();
        
    }
    public static void DeactivateNews()
    {
        //NewsCount.text = GameManager.Instance.player.UnreadNewsCount.ToString();
        NewsNotify.SetActive(false);       

    }
    public void ShowDailyQuests()
    {
        string sourceScene = string.Empty;

        if (SceneManager.GetSceneByName("HomeScene").IsValid())
            sourceScene = "HomeScene";
        else
            sourceScene = "MapScene";
            
        string targetScene = "DailyScene";

        UISceneInitializer.UserInfo.SetActive(false);
        UISceneInitializer.Status.SetActive(false);
        UISceneInitializer.HMButton.SetActive(false);

        GameManager.Instance.callingScene = sourceScene;
        GameManager.Instance.RefreshScene(sourceScene, targetScene);
    }

    public void ShowNews()
    {
        string sourceScene = string.Empty;

        if (SceneManager.GetSceneByName("HomeScene").IsValid())
            sourceScene = "HomeScene";
        else
            sourceScene = "MapScene";

        string targetScene = "ShipsLogScene";

        UISceneInitializer.UserInfo.SetActive(false);
        UISceneInitializer.Status.SetActive(false);
        UISceneInitializer.HMButton.SetActive(false);

        GameManager.Instance.callingScene = sourceScene;
        GameManager.Instance.RefreshScene(sourceScene, targetScene);
    }

    public void ShowInventory()
    {
        string sourceScene = string.Empty;

        if (SceneManager.GetSceneByName("HomeScene").IsValid())
            sourceScene = "HomeScene";
        else
            sourceScene = "MapScene";

        string targetScene = "InventoryScene";

        UISceneInitializer.UserInfo.SetActive(false);
        UISceneInitializer.Status.SetActive(false);
        UISceneInitializer.HMButton.SetActive(false);

        GameManager.Instance.callingScene = sourceScene;
        GameManager.Instance.RefreshScene(sourceScene, targetScene);
    }

    public void ShowHeroes()
    {
        string sourceScene = string.Empty;

        if (SceneManager.GetSceneByName("HomeScene").IsValid())
            sourceScene = "HomeScene";
        else
            sourceScene = "MapScene";

        string targetScene = "HeroesScene";

        UISceneInitializer.UserInfo.SetActive(false);
        UISceneInitializer.Status.SetActive(false);
        UISceneInitializer.HMButton.SetActive(false);

        GameManager.Instance.callingScene = sourceScene;
        GameManager.Instance.RefreshScene(sourceScene, targetScene);
    }

}
