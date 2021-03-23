using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDirView : MonoBehaviour
{
    [SerializeField] Transform arrowTransform;
    
    private Camera cam;
    private bool mustRender = true;
    private ILightingFacade lightingFacade;

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("lightDirCam").GetComponent<Camera>();
        lightingFacade = ServiceLocator.Instance.GetService<ILightingFacade>();
        lightingFacade.onChanged += () => mustRender = true;
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
        arrowTransform.LookAt(arrowTransform.position + lightingFacade.GetLightDirVec());
        cam.Render();
    }
}
