using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitializeManagerScene : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {

        if (!SceneManager.GetSceneByName("HomeScene").IsValid())
            SceneManager.LoadScene("HomeScene", LoadSceneMode.Additive);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
