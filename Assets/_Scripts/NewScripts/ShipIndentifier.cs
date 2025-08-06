using UnityEngine;

public class ShipIndentifier : MonoBehaviour
{
    [SerializeField] ShipRuntimeSetSO shipRuntimeSetSO;

    void OnEnable()
    {
        shipRuntimeSetSO.AddItem(this);
    }
    void OnDisable()
    {
        shipRuntimeSetSO.RemoveItem(this);
    }
}
