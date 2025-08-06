/*
Set this on an empty game object positioned at (0,0,0) and attach your active camera.
The script only runs on mobile devices or the remote app.
*/

using UnityEngine;

class ScrollAndPinch : MonoBehaviour
{
//#if UNITY_IOS || UNITY_ANDROID
    public Camera Camera;
    public bool Rotate;
    protected Plane Plane;

    private void Awake()
    {
        if (Camera == null)
            Camera = Camera.main;
    }

    //private static bool IsPointerOverUIObject()
    //{
    //    UnityEngine.EventSystems.PointerEventData eventDataCurrentPosition = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
    //    eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
    //    List<UnityEngine.EventSystems.RaycastResult> results = new List<UnityEngine.EventSystems.RaycastResult>();
    //    UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
    //    return results.Count > 0;
    //}

    private void Update()
    {

        //Update Plane
        if (Input.touchCount >= 1)
            Plane.SetNormalAndPosition(transform.up, transform.position);

        var Delta1 = Vector3.zero;
        var Delta2 = Vector3.zero;

        //Scroll
        if (Input.touchCount == 1)
        {
            //if (GameManager.Instance.IsPointerOverUIObject() == true)
            //    return;
            

            Delta1 = PlanePositionDelta(Input.GetTouch(0));

            Vector3 newPosition = Camera.transform.position + Delta1;

            //Debug.Log($"{Camera.transform.position}");
            //float x = Camera.transform.position.x;
            //float y = Camera.transform.position.y;
            //float z = Camera.transform.position.z;

            if (newPosition.x > 7f)
            {
                //Camera.transform.position = new Vector3(7f, y, z);
                return;
            }

            else if (newPosition.x < 0f)
            {
                //Camera.transform.position = new Vector3(0f, y, z);
                return;
            }

            else if (newPosition.z < -23f)
            {
                //Camera.transform.position = new Vector3(x, y, -11f);
                return;
            }

            else if (newPosition.z > 23f)
            {
                //Camera.transform.position = new Vector3(x, y, -10f);
                return;
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                Camera.transform.Translate(Delta1 * Time.deltaTime * 12f, Space.World);
                if (MapSceneInitializer.TileDialog != null)
                    MapSceneInitializer.TileDialog.SetActive(false);
            }

        }
        //Pinch
        else if (Input.touchCount >= 2)
        {
            //var pos1  = PlanePosition(Input.GetTouch(0).position);
            //var pos2  = PlanePosition(Input.GetTouch(1).position);
            //var pos1b = PlanePosition(Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition);
            //var pos2b = PlanePosition(Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition);

            ////calc zoom
            //var zoom = Vector3.Distance(pos1, pos2) /
            //           Vector3.Distance(pos1b, pos2b);

            ////edge case
            //if (zoom == 0 || zoom > 10)
            //    return;

            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            //Move cam amount the mid ray
            //Camera.transform.position = Vector3.LerpUnclamped(pos1, Camera.transform.position, 1 / zoom);
            Camera.orthographicSize += deltaMagnitudeDiff * 0.005f;

            if (Camera.orthographicSize > 8f)
                Camera.orthographicSize = 8f;

            if (Camera.orthographicSize < 4f)
                Camera.orthographicSize = 4f;

            //if (Rotate && pos2b != pos2)
            //    Camera.transform.RotateAround(pos1, Plane.normal, Vector3.SignedAngle(pos2 - pos1, pos2b - pos1b, Plane.normal));
        }

    }

    protected Vector3 PlanePositionDelta(Touch touch)
    {
        //not moved
        if (touch.phase != TouchPhase.Moved)
            return Vector3.zero;

        //delta
        var rayBefore = Camera.ScreenPointToRay(touch.position - touch.deltaPosition);
        var rayNow = Camera.ScreenPointToRay(touch.position);
        if (Plane.Raycast(rayBefore, out var enterBefore) && Plane.Raycast(rayNow, out var enterNow))
            return rayBefore.GetPoint(enterBefore) - rayNow.GetPoint(enterNow);

        //not on plane
        return Vector3.zero;
    }

    protected Vector3 PlanePosition(Vector2 screenPos)
    {
        //position
        var rayNow = Camera.ScreenPointToRay(screenPos);
        if (Plane.Raycast(rayNow, out var enterNow))
            return rayNow.GetPoint(enterNow);

        return Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + transform.up);
    }
//#endif
}