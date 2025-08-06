using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
public class CameraController : MonoBehaviour
{
    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private Transform myUnit;
    [SerializeField] private UnitManager unitManager;
    [SerializeField] CinemachineCamera followCamera;

    private Transform currentHeroTarget;
    private bool hasCameraDroped = false;
    private bool alreadyFollowing = false;
    private bool hasTargetAssigned = false;

    void OnEnable()
    {
        SetCameraTargetEvent.Instance += OnHeroSpawned;
        ResetCameraFollowEvent.Instance += OnCameraReseted;
        SetPlayerReferenceEvent.Instance += OnPlayerSpawned;
    }
    void OnDisable()
    {
        SetCameraTargetEvent.Instance -= OnHeroSpawned;
        ResetCameraFollowEvent.Instance -= OnCameraReseted;
        SetPlayerReferenceEvent.Instance -= OnPlayerSpawned;
    }
    private void OnPlayerSpawned(Transform player,string shipID)
    {
        myUnit = player;
        if(hasTargetAssigned) return;
        // Transform followTarget = GetGridUnderTheShip();
        followCamera.Follow = myUnit;
    }
    private Transform GetGridUnderTheShip()
    {
        Vector3Int myUnitCordinates = hexGrid.GetClosestHex(myUnit.position);
        Transform playerOnHex = hexGrid.GetTileAt(myUnitCordinates).transform;
        return playerOnHex;
    }
    private void OnHeroSpawned(Transform target, bool followPlayerPanelBtnClicked)
    {
        if (followCamera == null) return;
  
        if (hasCameraDroped && !followPlayerPanelBtnClicked) return;

        if (target == null && currentHeroTarget != null) // droping camera by not assigning targhet
        {
            Vector3Int playerCordinates = hexGrid.GetClosestHex(currentHeroTarget.position);
            Transform hexPosition = hexGrid.GetTileAt(playerCordinates).transform;
            followCamera.Follow = hexPosition;
            hasCameraDroped = true;
        }
        else if (target != null && !alreadyFollowing || followPlayerPanelBtnClicked)
        {
            followCamera.Follow = target;
            currentHeroTarget = target;
            hasCameraDroped = false;
            alreadyFollowing = true;
            hasTargetAssigned = true;
            SaveCameraSession(target);

        }
    }
    private void OnCameraReseted()
    {
        currentHeroTarget = null;
        hasCameraDroped = false;
        alreadyFollowing = false;
    }
    private void SaveCameraSession(Transform target)
    {
        if (target.TryGetComponent<HeroUnit>(out var heroUnit))
        {
            HeroSessionManager.SetFollowedHero(heroUnit.HeroId);
        }
    }

}
