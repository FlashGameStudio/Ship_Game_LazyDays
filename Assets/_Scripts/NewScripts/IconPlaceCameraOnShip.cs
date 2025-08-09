using TMPro;
using UnityEngine;

public class IconPlaceCameraOnShip : MonoBehaviour
{
    [SerializeField] private TMP_Text tileDistanceText;

    public void UpdateDistanceUI(int distance)
    {
        tileDistanceText.SetText(distance.ToString());
    }
}
