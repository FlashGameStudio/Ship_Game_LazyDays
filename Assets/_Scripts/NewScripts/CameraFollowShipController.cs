using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CameraFollowShipController : MonoBehaviour
{
    [SerializeField] private GameObject distanceUI;
    [SerializeField] private string mainShipID = "7";
    [SerializeField] private int minTileDistance = 3;
    [SerializeField] private Button shipFollowButton;
    [SerializeField] private TMP_Text tileDistanceText;
    [SerializeField] private RectTransform pointingArrow;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Camera cam;

    private float tileDistanceFromShip;
    private Transform mainShip;
    private bool iconActivated;
    [SerializeField] Transform cameraPointingTransform;
    bool rotated = false;

    void OnEnable()
    {
        //distanceUI.SetActive(false);
        SetPlayerReferenceEvent.Instance += OnMainShipReferenceSet;
        SetCameraPoinitingTransformEvent.Instance += OnCameraPointingTransformUpdated;
        shipFollowButton.onClick.AddListener(OnCameraFollowTargetButtonPressed);

    }
    void OnDisable()
    {
        SetPlayerReferenceEvent.Instance -= OnMainShipReferenceSet;
        SetCameraPoinitingTransformEvent.Instance -= OnCameraPointingTransformUpdated;
        shipFollowButton.onClick.RemoveListener(OnCameraFollowTargetButtonPressed);
    }
    private void Update()
    {
        PointArrowTowardsShip(mainShip);
        if (cameraPointingTransform == null || mainShip == null) return;

        tileDistanceFromShip = GetDistance(mainShip, cameraPointingTransform);
        UpdateDistanceUI(tileDistanceFromShip);

        if (tileDistanceFromShip >= minTileDistance && !iconActivated)
        {
            iconActivated = true;
            distanceUI.SetActive(true);
        }
        else if (tileDistanceFromShip < minTileDistance && iconActivated)
        {
            iconActivated = false;
            distanceUI.SetActive(false);
        }
    }
    private void OnCameraPointingTransformUpdated(Transform target)
    {
        cameraPointingTransform = target;
    }
    private float GetDistance(Transform mainShip, Transform cameraPointingTile)
    {
        Vector3 shipPosition = new Vector3(mainShip.position.x, 0, mainShip.position.z);
        Vector3 tilePosition = new Vector3(cameraPointingTile.position.x, 0, cameraPointingTile.position.z);
        return Vector3.Distance(shipPosition, tilePosition);
    }
    private void OnMainShipReferenceSet(Transform targetTransform, string shipID)
    {
        if (mainShipID == shipID)
            mainShip = targetTransform;
    }
    private void OnCameraFollowTargetButtonPressed()
    {
        SetCameraTargetEvent.Instance?.Invoke(mainShip, true);
        ResetCameraFollowEvent.Instance?.Invoke();
        distanceUI.SetActive(false);
        iconActivated = false;
    }
    private void UpdateDistanceUI(float distance)
    {
        tileDistanceText.SetText($"{distance.ToString("F0")}m");
    }
    private void PointArrowTowardsShip(Transform mainShip)
    {
        if (!mainShip || !pointingArrow || !canvas || !cam) return;

        Vector3 worldToScenePos = cam.WorldToScreenPoint(mainShip.position);

        // Convert screen position to local position in the arrow's parent space
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            pointingArrow.parent as RectTransform,
            worldToScenePos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
            out Vector2 localPos
        );


        Vector2 directionArrowToShip = localPos - (Vector2)pointingArrow.localPosition;

        float angle = Mathf.Atan2(directionArrowToShip.y, directionArrowToShip.x) * Mathf.Rad2Deg;

        // Apply rotation (arrow points up by default)
        pointingArrow.localRotation = Quaternion.Euler(0, 0, angle - 90f);
    }

}
public class SetCameraPoinitingTransformEvent
{
    public static Action<Transform> Instance;
}