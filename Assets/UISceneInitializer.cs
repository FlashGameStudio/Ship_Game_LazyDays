using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UISceneInitializer : MonoBehaviour
{
    [SerializeField]
    public GameObject userInfo;
    [SerializeField]
    public GameObject status;
    [SerializeField]
    public GameObject hmButton;


    // Start is called before the first frame update
    [SerializeField]
    public static GameObject UserInfo { get; private set; }
    [SerializeField]
    public static GameObject Status { get; private set; }
    [SerializeField]
    public static GameObject HMButton { get; private set; }

    public static GameObject EventsButton { get; private set; }

    public static GameObject PlayerActivityButton { get; private set; }

    [SerializeField]
    public GameObject eventsButton;
    [SerializeField]
    public GameObject huntButton;
    [SerializeField]
    public GameObject playerActivityButton;

    [SerializeField]
    public Image walkIcon;
    [SerializeField]
    public Image walkBackIcon;

    void Awake()
    {
        UserInfo = userInfo;
        Status = status;
        HMButton = hmButton;
        EventsButton = eventsButton;
        PlayerActivityButton = playerActivityButton;
    }

    void Start()
    {
        int currentTime = 0;

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log($"update enter UI scene method");
        if (GameManager.Instance.currentScene != "HomeScene" && GameManager.Instance.currentScene != "MapScene")
        {
            HideMapMovementIcon();
        }
        else
        {
            //eventsButton.SetActive(true);
            huntButton.SetActive(true);
        }
    }

    public void HideMapMovementIcon()
    {
        huntButton.SetActive(false);
    }

    public void HidePlayerActivityIcon()
    {
        playerActivityButton.SetActive(false);
    }


    }
