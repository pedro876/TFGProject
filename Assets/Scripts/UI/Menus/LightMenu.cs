using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LightMenu : MonoBehaviour
{
    [SerializeField] TMP_InputField powerField;
    [SerializeField] Toggle fogToggle;
    [SerializeField] TextMeshProUGUI rotationText;
    [SerializeField] TextMeshProUGUI inclinationText;
    [SerializeField] Slider rotationSlider;
    [SerializeField] Slider inclinationSlider;
    [SerializeField] RawImage lightDirImg;

    private Camera lightDirCam;
    private ILightingFacade lightingFacade;

    private void Start()
    {
        lightingFacade = ServiceLocator.Instance.GetService<ILightingFacade>();
        lightDirCam = GameObject.FindGameObjectWithTag("lightDirCam").GetComponent<Camera>();
        lightDirImg.texture = lightDirCam.targetTexture;
        lightDirImg.color = Color.white;

        GetOriginalData();
        LinkData();
    }

    private void GetOriginalData()
    {
        powerField.text = "" + lightingFacade.GetFogPower();
        fogToggle.isOn = lightingFacade.IsFogActive();
        Vector2 lightDir = lightingFacade.GetLightDir();
        rotationText.text = "" + Mathf.RoundToInt(lightDir.x);
        inclinationText.text = "" + Mathf.RoundToInt(lightDir.y);
        rotationSlider.value = Mathf.RoundToInt(lightDir.x);
        inclinationSlider.value = Mathf.RoundToInt(lightDir.y);
    }

    private void LinkData()
    {
        powerField.onValueChanged.AddListener((val) =>
        {
            if (float.TryParse(val.Replace(".", ","), out float v))
            {
                lightingFacade.SetFogPower(v);
            }
        });

        fogToggle.onValueChanged.AddListener((val) =>
        {
            lightingFacade.ActivateFog(val);
        });

        rotationSlider.onValueChanged.AddListener((rotation) =>
        {
            float inclination = inclinationSlider.value;
            lightingFacade.SetLightDir(rotation, inclination);
            rotationText.text = "" + Mathf.RoundToInt(rotation);
        });

        inclinationSlider.onValueChanged.AddListener((inclination) =>
        {
            float rotation = rotationSlider.value;
            lightingFacade.SetLightDir(rotation, inclination);
            inclinationText.text = "" + Mathf.RoundToInt(inclination);
        });
    }
}
