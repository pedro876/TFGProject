using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CoordSphere : MonoBehaviour
{
    [SerializeField] bool axisX;
    [SerializeField] bool axisY;
    [SerializeField] bool axisZ;

    private Transform cam;
    private TextMeshProUGUI text;

    private IViewFacade viewFacade;
    private IRegionFacade regionFacade;

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("3dCam").transform;
        text = GetComponentInChildren<TextMeshProUGUI>();

        viewFacade = ServiceLocator.Instance.GetService<IViewFacade>();
        regionFacade = ServiceLocator.Instance.GetService<IRegionFacade>();

        viewFacade.onChanged += LookAtCam;
        regionFacade.onChanged += UpdateInfo;
        UpdateInfo();
        LookAtCam();
    }

    private void UpdateInfo()
    {
        Vector3 pos = transform.position;
        Vector3 finalPos = regionFacade.TransformToRegion(ref pos);

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
