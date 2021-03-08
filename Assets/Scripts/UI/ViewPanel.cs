using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ViewPanel : MonoBehaviour
{
    [SerializeField] TMP_InputField minXField;
    [SerializeField] TMP_InputField maxXField;
    [SerializeField] TMP_InputField minYField;
    [SerializeField] TMP_InputField maxYField;
    [SerializeField] TMP_InputField minZField;
    [SerializeField] TMP_InputField maxZField;

    [SerializeField] Transform axes;
    [SerializeField] Toggle showAxes;

    private void Start()
    {
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

        showAxes.onValueChanged.AddListener((val) => axes.gameObject.SetActive(val));
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
