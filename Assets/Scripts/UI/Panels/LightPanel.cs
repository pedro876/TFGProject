using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LightPanel : MonoBehaviour
{
    [SerializeField] TMP_InputField powerField;
    [SerializeField] Toggle fogToggle;
    [SerializeField] TextMeshProUGUI rotationText;
    [SerializeField] TextMeshProUGUI inclinationText;
    [SerializeField] Slider rotationSlider;
    [SerializeField] Slider inclinationSlider;
    [SerializeField] RawImage lightDirImg;

    private Camera lightDirCam;

    private PostProcess pp;

    private void Start()
    {
        pp = FindObjectOfType<PostProcess>();
        lightDirCam = GameObject.FindGameObjectWithTag("lightDirCam").GetComponent<Camera>();
        lightDirImg.texture = lightDirCam.targetTexture;
        lightDirImg.color = Color.white;

        powerField.text = ""+PostProcess.fogPower;
        powerField.onValueChanged.AddListener((val) =>
        {
            float v;
            val = val.Replace(".", ",");
            if(float.TryParse(val, out v))
            {
                PostProcess.fogPower = v;
                pp.Render();
            }
        });

        fogToggle.isOn = PostProcess.fog;
        fogToggle.onValueChanged.AddListener((val) =>
        {
            PostProcess.fog = val;
            pp.Render();
        });

        Vector2 lightDir = PostProcess.GetLightDir();
        rotationText.text = "" + Mathf.RoundToInt(lightDir.x);
        inclinationText.text = "" + Mathf.RoundToInt(lightDir.y);
        rotationSlider.value = Mathf.RoundToInt(lightDir.x);
        inclinationSlider.value = Mathf.RoundToInt(lightDir.y);
        rotationSlider.onValueChanged.AddListener((val) =>
        {
            float inclination = inclinationSlider.value;
            PostProcess.SetLightDir(val, inclination);
            pp.Render();
            rotationText.text = "" + Mathf.RoundToInt(val);
        });

        inclinationSlider.onValueChanged.AddListener((val) =>
        {
            float rotation = rotationSlider.value;
            PostProcess.SetLightDir(rotation, val);
            pp.Render();
            inclinationText.text = "" + Mathf.RoundToInt(val);
        });
    }
}
