using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapHomeButtonScript : MonoBehaviour
{
    public TMPro.TMP_Text buttonText;
    // Start is called before the first frame update
    void Start()
    {
        if (SceneManager.GetSceneByName("HomeScene").IsValid())
        {
            buttonText.text = "Map";
        }
        else
        {
            buttonText.text = "Home";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowScene()
    {
        if (SceneManager.GetSceneByName("HomeScene").IsValid())
        {
            if (false)
            {            
            }
            else
            {
                SceneManager.UnloadSceneAsync("HomeScene");
                StartCoroutine(SceneLoadingScreen.Instance.LoadSceneAsyn("MapScene"));
                buttonText.text = "Home";
            }

            GameManager.Instance.mapMonstersLoaded = false;
            GameManager.Instance.mapPlayersLoaded = false;

        }
        else
        {
            SceneManager.UnloadSceneAsync("MapScene");
            StartCoroutine(SceneLoadingScreen.Instance.LoadSceneAsyn("HomeScene"));
            buttonText.text = "Map";
            GameManager.Instance.mapMonstersLoaded = false;
            GameManager.Instance.mapPlayersLoaded = false;
        }
            //SceneManager.LoadScene("UI", LoadSceneMode.Additive);
        
    }
}
