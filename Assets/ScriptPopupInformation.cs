using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScriptPopupInformation : MonoBehaviour
{
    [SerializeField]
    public TMP_Text popupTitle;
    [SerializeField]
    public TMP_Text popupMessage;
    [SerializeField]
    public Button buttonOK;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ClosePopup()
    {
       this.gameObject.SetActive(false);
    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //}
}
