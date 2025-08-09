using TMPro;
using UnityEngine;

public class HeroDistanceFromShipTracker : MonoBehaviour
{
    [SerializeField] private TMP_Text tileDistanceText;

    public void UpdateDistanceUI(int distance)
    {
        tileDistanceText.SetText(distance.ToString());
    }
}
