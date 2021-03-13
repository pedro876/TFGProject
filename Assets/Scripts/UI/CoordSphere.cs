using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CoordSphere : MonoBehaviour
{
    Transform cam;
    [SerializeField] bool axisX;
    [SerializeField] bool axisY;
    [SerializeField] bool axisZ;

    TextMeshProUGUI text;

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("3dCam").transform;
        text = GetComponentInChildren<TextMeshProUGUI>();
        ViewController.onChanged += () =>
        {
            LookAtCam();
            UpdateInfo();
        };
    }

    private void UpdateInfo()
    {
        Vector3 pos = transform.position;
        Vector3 finalPos = ViewController.TransformToRegion(ref pos);

        const float mult = 100f;
        if (axisX) text.text = "x: " + Mathf.RoundToInt(finalPos.x * mult) / mult;
        else if (axisY) text.text = "y: " + Mathf.RoundToInt(finalPos.y * mult) / mult;
        else if (axisZ) text.text = "z: " + Mathf.RoundToInt(finalPos.z * mult) / mult;
        else
        {
            text.text = "" + Mathf.RoundToInt(finalPos.x * mult) / mult + " " + Mathf.RoundToInt(finalPos.y * mult) / mult + " " + Mathf.RoundToInt(finalPos.z * mult) / mult;
        }
    }

    private void LookAtCam()
    {
        transform.LookAt(transform.position-cam.forward);
    }
}
