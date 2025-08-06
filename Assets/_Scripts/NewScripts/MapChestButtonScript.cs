using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapChestButtonScript : MonoBehaviour
{
    [SerializeField] GameObject chestCanvas;
    [SerializeField] GameObject chestModels;
    public void ActivateChestCanvas(bool value)
    {
        chestCanvas.SetActive(value);
        chestModels.SetActive(value);
    }
}
