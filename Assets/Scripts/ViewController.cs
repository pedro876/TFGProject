using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;


public class ViewController : MonoBehaviour, IPointerDownHandler
{

    [SerializeField] bool DEBUG = false;
    [SerializeField] GameObject pressSpaceText;
    bool focused = false;

    [SerializeField] Camera Cam3D;
    public static Camera cam;
    public static Transform camTransform;

    [Header("Move variables")]
    [SerializeField] float orbitSpeed = 1f;
    [SerializeField] float rotSpeed = 1f;
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float moveAc = 1f;

    public static Vector2 regionX = new Vector2(-5f, 5f);
    public static Vector2 regionY = new Vector2(-5f, 5f);
    public static Vector2 regionZ = new Vector2(-5f, 5f);
    private static Vector3 regionScale = Vector3.one;
    private static Vector3 regionCenter = Vector3.zero;

    public static Vector3 nearTopLeft = Vector3.zero;
    public static Vector3 nearTopRight = Vector3.zero;
    public static Vector3 nearBottomRight = Vector3.zero;
    public static Vector3 nearBottomLeft = Vector3.zero;
    public static Vector3 farTopLeft = Vector3.zero;
    public static Vector3 farTopRight = Vector3.zero;
    public static Vector3 farBottomRight = Vector3.zero;
    public static Vector3 farBottomLeft = Vector3.zero;

    private float near = 0f;
    private float far = 0f;
    private bool ortographic = false;
    private float ortoSize = 0f;
    private float fov = 0f;

    private enum CamMode { Orbit, Fly }
    [SerializeField] CamMode camMode = CamMode.Orbit;

    float x, y;

    bool changed = false;
    public static event Action onChanged;

    bool firstFrame = true;

    private void Awake()
    {
        cam = Cam3D;
        pressSpaceText.SetActive(false);
        camTransform = cam.transform;
        camTransform.LookAt(Vector3.zero);
        CheckCamInfo();
        MoveCamera();
        CalculatePlanes();
        SetRegion(regionX.x, regionX.y, regionY.x, regionY.y, regionZ.x, regionZ.y);
    }

    private void LateUpdate()
    {
        changed = false;
        CheckFocus();
        CheckCamInfo();

        if (focused)
        {
            GetInput();
            MoveCamera();
        }
        CalculatePlanes();

        if (changed || firstFrame) onChanged?.Invoke();
        firstFrame = false;
    }

    #region CameraInfo

    private void CheckCamInfo()
    {
        if (cam.nearClipPlane != near)
        {
            near = cam.nearClipPlane;
            changed = true;
        }
        if (cam.farClipPlane != far)
        {
            far = cam.farClipPlane;
            changed = true;
        }
        if (cam.orthographic != ortographic)
        {
            ortographic = cam.orthographic;
            changed = true;
        }
        if (ortographic)
        {
            if (ortoSize != cam.orthographicSize)
            {
                ortoSize = cam.orthographicSize;
                changed = true;
            }
        }
        else
        {
            if (fov != cam.fieldOfView)
            {
                fov = cam.fieldOfView;
                changed = true;
            }
        }
    }

    private void CalculatePlanes()
    {
        Vector3 nearCenter = camTransform.position + camTransform.forward * near;
        if (ortographic)
        {
            nearTopLeft = nearCenter - ortoSize * camTransform.right + camTransform.up * ortoSize;
            nearTopRight = nearCenter + ortoSize * camTransform.right + camTransform.up * ortoSize;
            nearBottomRight = nearCenter + ortoSize * camTransform.right - camTransform.up * ortoSize;
            nearBottomLeft = nearCenter - ortoSize * camTransform.right - camTransform.up * ortoSize;
            farTopLeft = nearTopLeft + camTransform.forward * (far-near);
            farTopRight = nearTopRight + camTransform.forward * (far - near);
            farBottomRight = nearBottomRight + camTransform.forward * (far-near);
            farBottomLeft = nearBottomLeft + camTransform.forward * (far - near);
        }
        else
        {
            Vector3 farCenter = camTransform.position + camTransform.forward * far;
            float fovRad = fov * Mathf.PI / 180f;
            float nearSize = Mathf.Tan(fovRad * 0.5f) * near;
            float farSize = Mathf.Tan(fovRad * 0.5f) * far;
            nearTopLeft = nearCenter - nearSize * camTransform.right + camTransform.up * nearSize;
            nearTopRight = nearCenter + nearSize * camTransform.right + camTransform.up * nearSize;
            nearBottomRight = nearCenter + nearSize * camTransform.right - camTransform.up * nearSize;
            nearBottomLeft = nearCenter - nearSize * camTransform.right - camTransform.up * nearSize;
            farTopLeft = farCenter - farSize * camTransform.right + camTransform.up * farSize;
            farTopRight = farCenter + farSize * camTransform.right + camTransform.up * farSize;
            farBottomRight = farCenter + farSize * camTransform.right - camTransform.up * farSize;
            farBottomLeft = farCenter - farSize * camTransform.right - camTransform.up * farSize;
        }
    }

    #endregion

    #region Motion

    private void GetInput()
    {
        x = Input.GetAxis("Mouse X");
        y = Input.GetAxis("Mouse Y");
        if (Mathf.Abs(x) > 0.001f || Mathf.Abs(y) > 0.001f) changed = true;
    }

    private void MoveCamera()
    {
        switch (camMode)
        {
            case CamMode.Orbit: OrbitMove(); break;
            case CamMode.Fly: FlyMove(); break;
        }
    }

    private void OrbitMove()
    {
        camTransform.LookAt(Vector3.zero);
        float degreesX = x * orbitSpeed * Time.deltaTime;
        camTransform.RotateAround(Vector3.zero, Vector3.up, degreesX);
        float degreesY = -y * orbitSpeed * Time.deltaTime;

        const float limit = 89.5f;
        float currentDegreesY = Vector3.SignedAngle(camTransform.position, Vector3.ProjectOnPlane(camTransform.position, Vector3.up), -camTransform.right);
        //limit view
        if(currentDegreesY + degreesY >= limit && degreesY > 0f)
        {
            degreesY = limit - currentDegreesY;
        } else if(currentDegreesY + degreesY <= -limit && degreesY < 0f)
        {
            degreesY = -limit - currentDegreesY;
        } 
        camTransform.RotateAround(Vector3.zero, camTransform.right, degreesY);

    }

    private void FlyMove()
    {

    }

    #endregion

    #region FocusControl

    private void CheckFocus()
    {
        if (focused &&
            (Cursor.visible ||
            Cursor.lockState != CursorLockMode.Locked ||
            Input.GetKeyDown(KeyCode.Space)))
        {
            OnChangeFocus(false);
        }
    }

    private void OnChangeFocus(bool f = true)
    {
        focused = f;
        pressSpaceText.SetActive(focused);
        Cursor.lockState = focused ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !focused;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!focused) OnChangeFocus(!focused);
    }

    #endregion

    #region Region

    public static void SetRegion(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
    {
        regionX = new Vector2(Mathf.Min(minX, maxX), Mathf.Max(minX, maxX));
        regionY = new Vector2(Mathf.Min(minY, maxY), Mathf.Max(minY, maxY));
        regionZ = new Vector2(Mathf.Min(minZ, maxZ), Mathf.Max(minZ, maxZ));

        regionScale = new Vector3(regionX.y - regionX.x, regionY.y - regionY.x, regionZ.y - regionZ.x);
        regionCenter = new Vector3((regionX.x + regionX.y) * 0.5f, (regionY.x + regionY.y) * 0.5f, (regionZ.x + regionZ.y) * 0.5f);
        onChanged?.Invoke();
    }

    public static Vector3 TransformToRegion(ref Vector3 pos)
    {
        return (new Vector3(pos.x * regionScale.x, pos.z * regionScale.y, pos.y * regionScale.z)) + regionCenter;
    }

    #endregion

    private void OnDrawGizmos()
    {
        if (DEBUG)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(nearTopLeft, 1f);
            Gizmos.DrawSphere(nearTopRight, 1f);
            Gizmos.DrawSphere(nearBottomRight, 1f);
            Gizmos.DrawSphere(nearBottomLeft, 1f);
            Gizmos.DrawSphere(farTopLeft, 1f);
            Gizmos.DrawSphere(farTopRight, 1f);
            Gizmos.DrawSphere(farBottomRight, 1f);
            Gizmos.DrawSphere(farBottomLeft, 1f);
        }
    }
}
