using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDirView : MonoBehaviour
{
    Camera cam;
    [SerializeField] Transform arrowTransform;

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("lightDirCam").GetComponent<Camera>();
        UpdateDir();
        PostProcess.onLightDirChanged += UpdateDir;
    }

    private void UpdateDir()
    {
        arrowTransform.LookAt(arrowTransform.position + PostProcess.GetLightDirVec());
        cam.Render();
    }
}
