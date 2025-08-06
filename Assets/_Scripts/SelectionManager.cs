using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SelectionManager : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;

    public LayerMask selectionMask;


    public UnityEvent<GameObject> OnUnitSelected;
    public UnityEvent<GameObject> OnTerrainSelected;
    public UnityEvent<GameObject> OnNPCSelected;
    public UnityEvent<GameObject> OnAttackOnNPCSelected;


    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    public void HandleClick(Vector3 mousePosition)
    {
        if (GameManager.Instance.IsPointerOverUIObject() != true)
        {
            GameObject result;
            if (FindTarget(mousePosition, out result))
            {
                if (HeroUnitSelected(result, out HeroUnit resultedHeroUnit))
                {
                    MovingHeroUnitSelectionEvent.Instance?.Invoke(resultedHeroUnit);
                }
                else if (UnitSelected(result))
                {
                    //Debug.Log("pc selected");
                    OnUnitSelected?.Invoke(result);
                }
                else if (NPCSelected(result))
                {
                    //Debug.Log("npc selected");
                    OnNPCSelected?.Invoke(result);
                    OnAttackOnNPCSelected?.Invoke(result);
                }
                else if (TerrainSelected(result))
                {
                    Debug.Log("terrain selected");
                    OnTerrainSelected?.Invoke(result);
                    SetCameraTargetEvent.Instance?.Invoke(null, false);
                    HeroSessionManager.SetFollowedHero(string.Empty);
                }
               
            }
        }
    }

    private bool UnitSelected(GameObject result)
    {
        return result.GetComponent<Unit>() != null;
    }

    private bool NPCSelected(GameObject result)
    {
        return result.GetComponent<NPC>() != null;
    }
    private bool HeroUnitSelected(GameObject result, out HeroUnit heroUnit)
    {
        heroUnit = result.GetComponent<HeroUnit>();
        return heroUnit != null;
    }
    private bool TerrainSelected(GameObject result)
    {
        return result.GetComponent<Hex>() != null;
    }

    private bool FindTarget(Vector3 mousePosition, out GameObject result)
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        bool isOverUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        if (Physics.Raycast(ray, out hit, 100, selectionMask) && !isOverUI)
        {
            result = hit.collider.gameObject;
            return true;
        }
        result = null;
        return false;
    }
}
