using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ViewController : MonoBehaviour, IPointerDownHandler
{
    bool focused = false;

    [SerializeField] Camera cam;
    Transform camTransform;

    [Header("Move variables")]
    [SerializeField] float orbitSpeed = 1f;
    [SerializeField] float rotSpeed = 1f;
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float moveAc = 1f;
    //RawImage image;
    

    private enum CamMode { Orbit, Fly }
    [SerializeField] CamMode camMode = CamMode.Orbit;

    float x, y;

    private void Start()
    {
        camTransform = cam.transform;
        camTransform.LookAt(Vector3.zero);
        //image = GetComponent<RawImage>();
    }

    private void Update()
    {
        if (focused && (Cursor.visible || Cursor.lockState != CursorLockMode.Locked)) OnChangeFocus();

        if (focused)
        {
            x = Input.GetAxis("Mouse X");
            y = Input.GetAxis("Mouse Y");
            MoveCamera();
        }
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

        const float limit = 88f;
        float currentDegreesY = Vector3.SignedAngle(camTransform.position, Vector3.ProjectOnPlane(camTransform.position, Vector3.up), -camTransform.right);
        //limit view
        if(currentDegreesY + degreesY >= limit && degreesY > 0f)
        {
            degreesY = limit - currentDegreesY;
        } else if(currentDegreesY + degreesY <= -limit && degreesY < 0f)
        {
            degreesY = -limit - currentDegreesY;
        } else
        {
            camTransform.RotateAround(Vector3.zero, camTransform.right, degreesY);
        }
    }

    private void FlyMove()
    {

    }

    private void OnChangeFocus()
    {
        focused = !focused;
        Cursor.lockState = focused ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !focused;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!focused) OnChangeFocus();
    }
}
