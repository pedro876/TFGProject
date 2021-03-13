using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDirView : MonoBehaviour
{
    Camera cam;
    [SerializeField] Transform arrowTransform;
    bool mustRender = true;

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("lightDirCam").GetComponent<Camera>();
        PostProcess.onLightDirChanged += ()=>mustRender = true;
    }

    private void Update()
    {
        if (mustRender)
        {
            mustRender = false;
            UpdateDir();
        }
    }

    private void UpdateDir()
    {
        arrowTransform.LookAt(arrowTransform.position + PostProcess.GetLightDirVec());
        cam.Render();
    }
}
