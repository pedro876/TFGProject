﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ViewPanel : MonoBehaviour
{
    [Header("Region")]
    [SerializeField] TMP_InputField minXField;
    [SerializeField] TMP_InputField maxXField;
    [SerializeField] TMP_InputField minYField;
    [SerializeField] TMP_InputField maxYField;
    [SerializeField] TMP_InputField minZField;
    [SerializeField] TMP_InputField maxZField;
    [SerializeField] Toggle clampToRegion;

    [Header("Axes")]
    [SerializeField] Transform axes;
    private RawImage axesImage;
    [SerializeField] Toggle showAxes;
    [SerializeField] Slider axesOpacity;

    [Header("Camera")]
    [SerializeField] Toggle perspectiveView;
    [SerializeField] TMP_InputField nearField;
    [SerializeField] TMP_InputField farField;
    [SerializeField] TMP_InputField fovField;
    [SerializeField] TMP_InputField ortographicSizeField;

    [Header("Channels")]
    [SerializeField] Toggle antialiasingToggle;
    [SerializeField] Toggle depthToggle;
    [SerializeField] Toggle normalToggle;
    [SerializeField] Toggle lightToggle;

    private PostProcess pp;


    private void Start()
    {
        pp = FindObjectOfType<PostProcess>();
        minXField.text = ""+ViewController.regionX.x;
        minYField.text = ""+ViewController.regionY.x;
        minZField.text = ""+ViewController.regionZ.x;
        maxXField.text = ""+ViewController.regionX.y;
        maxYField.text = ""+ViewController.regionY.y;
        maxZField.text = ""+ViewController.regionZ.y;

        minXField.onValueChanged.AddListener((s) => UpdateRegion());
        maxXField.onValueChanged.AddListener((s) => UpdateRegion());
        minYField.onValueChanged.AddListener((s) => UpdateRegion());
        maxYField.onValueChanged.AddListener((s) => UpdateRegion());
        minZField.onValueChanged.AddListener((s) => UpdateRegion());
        maxZField.onValueChanged.AddListener((s) => UpdateRegion());

        clampToRegion.isOn = ViewController.GetClampToRegion();
        clampToRegion.onValueChanged.AddListener(ViewController.SetClampToRegion);

        axesImage = axes.GetComponentInChildren<RawImage>();
        showAxes.isOn = axes.gameObject.activeSelf;
        showAxes.onValueChanged.AddListener((val) => axes.gameObject.SetActive(val));
        axesOpacity.value = axesImage.color.a;
        axesOpacity.onValueChanged.AddListener((val) => axesImage.color = new Color(axesImage.color.r, axesImage.color.g, axesImage.color.b, val));
        perspectiveView.onValueChanged.AddListener((val) => ViewController.cam.orthographic = !val);
        nearField.text = ""+ViewController.cam.nearClipPlane;
        nearField.onValueChanged.AddListener((val) =>
        {
            val = val.Replace(".", ",");
            float v;
            if(float.TryParse(val, out v))
            {
                ViewController.cam.nearClipPlane = v;
            }
        });
        farField.text = "" + ViewController.cam.farClipPlane;
        farField.onValueChanged.AddListener((val) =>
        {
            val = val.Replace(".", ",");
            float v;
            if (float.TryParse(val, out v))
            {
                ViewController.cam.farClipPlane = v;
            }
        });
        ortographicSizeField.text = "" + ViewController.cam.orthographicSize;
        ortographicSizeField.onValueChanged.AddListener((val) =>
        {
            val = val.Replace(".", ",");
            float v;
            if (float.TryParse(val, out v))
            {
                ViewController.cam.orthographicSize = v;
            }
        });
        fovField.text = "" + ViewController.cam.fieldOfView;
        fovField.onValueChanged.AddListener((val) =>
        {
            val = val.Replace(".", ",");
            float v;
            if (float.TryParse(val, out v))
            {
                ViewController.cam.fieldOfView = v;
            }
        });

        antialiasingToggle.isOn = PostProcess.antialiasing;
        antialiasingToggle.onValueChanged.AddListener((val) =>
        {
            PostProcess.antialiasing = val;
            pp.UpdateDisplay();
        });
        if (depthToggle.isOn) PostProcess.display = PostProcess.Display.depth;
        if (normalToggle.isOn) PostProcess.display = PostProcess.Display.normals;
        if (lightToggle.isOn) PostProcess.display = PostProcess.Display.light;
        depthToggle.onValueChanged.AddListener((val) => {
            if (val)
            {
                PostProcess.display = PostProcess.Display.depth;
                pp.UpdateDisplay();
            }
        });
        normalToggle.onValueChanged.AddListener((val) => {
            if (val)
            {
                PostProcess.display = PostProcess.Display.normals;
                pp.UpdateDisplay();
            }
        });
        lightToggle.onValueChanged.AddListener((val) => {
            if (val)
            {
                PostProcess.display = PostProcess.Display.light;
                pp.UpdateDisplay();
            }
        });
    }

    private void UpdateRegion()
    {
        float minX, maxX, minY, maxY, minZ, maxZ;
        float.TryParse(minXField.text.Replace(".",","), out minX);
        float.TryParse(maxXField.text.Replace(".",","), out maxX);
        float.TryParse(minYField.text.Replace(".",","), out minY);
        float.TryParse(maxYField.text.Replace(".",","), out maxY);
        float.TryParse(minZField.text.Replace(".",","), out minZ);
        float.TryParse(maxZField.text.Replace(".", ","), out maxZ);
        ViewController.SetRegion(minX, maxX, minY, maxY, minZ, maxZ);
    }
}
