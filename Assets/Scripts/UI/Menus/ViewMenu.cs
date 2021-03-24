using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ViewMenu : MonoBehaviour
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
    [SerializeField] Toggle showAxes;
    [SerializeField] Slider axesOpacity;
    private RawImage axesImage;

    [Header("Camera")]
    [SerializeField] Toggle perspectiveView;
    [SerializeField] Toggle orbitToggle;
    [SerializeField] Toggle flyToggle;
    [SerializeField] TMP_InputField nearField;
    [SerializeField] TMP_InputField farField;
    [SerializeField] TMP_InputField fovField;
    [SerializeField] TMP_InputField ortographicSizeField;

    [Header("Channels")]
    [SerializeField] Toggle antialiasingToggle;
    [SerializeField] Toggle depthToggle;
    [SerializeField] Toggle normalToggle;
    [SerializeField] Toggle lightToggle;

    private IRegionFacade regionFacade;
    private IPostProcessFacade postProcessFacade;
    private IViewFacade viewFacade;

    private void Start()
    {
        regionFacade = ServiceLocator.Instance.GetService<IRegionFacade>();
        postProcessFacade = ServiceLocator.Instance.GetService<IPostProcessFacade>();
        viewFacade = ServiceLocator.Instance.GetService<IViewFacade>();
        axesImage = axes.GetComponentInChildren<RawImage>();

        GetOriginalInfo();
        LinkData();
    }

    private void GetOriginalInfo()
    {
        minXField.text = regionFacade.GetRegionX().x.ToString();
        minYField.text = regionFacade.GetRegionY().x.ToString();
        minZField.text = regionFacade.GetRegionZ().x.ToString();
        maxXField.text = regionFacade.GetRegionX().y.ToString();
        maxYField.text = regionFacade.GetRegionY().y.ToString();
        maxZField.text = regionFacade.GetRegionZ().y.ToString();

        nearField.text = viewFacade.Near.ToString();
        clampToRegion.isOn = regionFacade.IsRegionClamped();
        showAxes.isOn = axes.gameObject.activeSelf;
        axesOpacity.value = axesImage.color.a;
        farField.text = viewFacade.Far.ToString();
        ortographicSizeField.text = viewFacade.OrtographicSize.ToString();

        perspectiveView.SetIsOnWithoutNotify(!viewFacade.Ortographic);
        orbitToggle.SetIsOnWithoutNotify(viewFacade.IsUsingOrbitMove());
        flyToggle.SetIsOnWithoutNotify(viewFacade.IsUsingFlyMove());

        depthToggle.isOn = postProcessFacade.IsDisplayinDepth();
        normalToggle.isOn = postProcessFacade.IsDisplayinNormals();
        lightToggle.isOn = postProcessFacade.IsDisplayinLighting();

        fovField.text = viewFacade.Fov.ToString();
        antialiasingToggle.isOn = postProcessFacade.IsUsingAntialiasing();
    }

    private void LinkData()
    {
        minXField.onValueChanged.AddListener((s) => UpdateRegion());
        maxXField.onValueChanged.AddListener((s) => UpdateRegion());
        minYField.onValueChanged.AddListener((s) => UpdateRegion());
        maxYField.onValueChanged.AddListener((s) => UpdateRegion());
        minZField.onValueChanged.AddListener((s) => UpdateRegion());
        maxZField.onValueChanged.AddListener((s) => UpdateRegion());

        clampToRegion.onValueChanged.AddListener(regionFacade.ActivateRegionClamp);

        showAxes.onValueChanged.AddListener((val) => axes.gameObject.SetActive(val));

        axesOpacity.onValueChanged.AddListener((val) =>
            axesImage.color = new Color(axesImage.color.r, axesImage.color.g, axesImage.color.b, val)
        );

        perspectiveView.onValueChanged.AddListener((val) => viewFacade.Ortographic = !val);

        orbitToggle.onValueChanged.AddListener((val) =>
        {
            if (val)
                viewFacade.UseOrbitMove();
        });

        flyToggle.onValueChanged.AddListener((val) =>
        {
            if (val)
                viewFacade.UseFlyMove();
        });

        nearField.onValueChanged.AddListener((val) =>
        {
            if (float.TryParse(val.Replace(".", ","), out float v))
            {
                viewFacade.Near = Mathf.Max(0.01f, v);
            }
        });
        farField.onValueChanged.AddListener((val) =>
        {
            if (float.TryParse(val.Replace(".", ","), out float v))
            {
                viewFacade.Far = Mathf.Max(viewFacade.Near + 0.01f, v);
            }
        });
        ortographicSizeField.onValueChanged.AddListener((val) =>
        {
            if (float.TryParse(val.Replace(".", ","), out float v))
            {
                viewFacade.OrtographicSize = Mathf.Max(v, 0.1f);
            }
        });

        fovField.onValueChanged.AddListener((val) =>
        {
            if (float.TryParse(val.Replace(".", ","), out float v))
            {
                viewFacade.Fov = Mathf.Clamp(v, 1f, 180f);
            }
        });

        antialiasingToggle.onValueChanged.AddListener((val) => postProcessFacade.UseAntialiasing(val));
        depthToggle.onValueChanged.AddListener((val) => { if (val) postProcessFacade.DisplayDepth(); });
        normalToggle.onValueChanged.AddListener((val) => { if (val) postProcessFacade.DisplayNormals(); });
        lightToggle.onValueChanged.AddListener((val) => { if (val) postProcessFacade.DisplayLighting(); });
    }

    private void UpdateRegion()
    {
        float.TryParse(minXField.text.Replace(".", ","), out float minX);
        float.TryParse(maxXField.text.Replace(".", ","), out float maxX);
        float.TryParse(minYField.text.Replace(".", ","), out float minY);
        float.TryParse(maxYField.text.Replace(".", ","), out float maxY);
        float.TryParse(minZField.text.Replace(".", ","), out float minZ);
        float.TryParse(maxZField.text.Replace(".", ","), out float maxZ);
        regionFacade.SetRegion(minX, maxX, minY, maxY, minZ, maxZ);
    }
}
