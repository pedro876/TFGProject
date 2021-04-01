using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InterpretationMenu : MonoBehaviour
{
    [SerializeField] Toggle greaterThanToggle;
    [SerializeField] Toggle lessThanToggle;

    [SerializeField] Toggle xToggle;
    [SerializeField] Toggle yToggle;
    [SerializeField] Toggle zToggle;
    [SerializeField] Toggle thresholdToggle;
    [SerializeField] Toggle differenceToggle;
    [SerializeField] TextMeshProUGUI differenceVal;
    [SerializeField] Slider differenceSlider;
    [SerializeField] TMP_InputField thresholdField;

    [SerializeField] Toggle useAuto;

    private IMassFacade massFacade;

    private void Start()
    {
        massFacade = ServiceLocator.Instance.GetService<IMassFacade>();
        massFacade.onChanged += GetOriginalData;
        thresholdField.SetTextWithoutNotify(massFacade.Threshold.ToString());
        GetOriginalData();
        LinkData();
    }

    private void GetOriginalData()
    {
        greaterThanToggle.SetIsOnWithoutNotify(massFacade.Criterion == massFacade.GreaterCriterion);
        lessThanToggle.SetIsOnWithoutNotify(massFacade.Criterion == massFacade.LessCriterion);
        differenceToggle.SetIsOnWithoutNotify(massFacade.Criterion == massFacade.MinDifferenceCriterion);

        xToggle.SetIsOnWithoutNotify(massFacade.Variable == massFacade.VariableX);
        yToggle.SetIsOnWithoutNotify(massFacade.Variable == massFacade.VariableY);
        zToggle.SetIsOnWithoutNotify(massFacade.Variable == massFacade.VariableZ);
        thresholdToggle.SetIsOnWithoutNotify(massFacade.Variable == massFacade.VariableThreshold);

        differenceVal.text = (Mathf.RoundToInt(massFacade.MinDifference * 10f) / 10f).ToString();
        differenceSlider.SetValueWithoutNotify(massFacade.MinDifference);
        

        useAuto.isOn = massFacade.AutoMode;
    }

    private void LinkData()
    {
        greaterThanToggle.onValueChanged.AddListener((val) =>
        {
            if (val) massFacade.Criterion = massFacade.GreaterCriterion;
        });

        lessThanToggle.onValueChanged.AddListener((val) =>
        {
            if (val) massFacade.Criterion = massFacade.LessCriterion;
        });

        differenceToggle.onValueChanged.AddListener((val) =>
        {
            if (val) massFacade.Criterion = massFacade.MinDifferenceCriterion;
        });
        differenceSlider.onValueChanged.AddListener((val) =>
        {
            massFacade.MinDifference = val;
            differenceVal.text = "" + Mathf.RoundToInt(val * 10f) / 10f;
        });
        xToggle.onValueChanged.AddListener((val) =>
        {
            if (val) massFacade.Variable = massFacade.VariableX;
        });

        yToggle.onValueChanged.AddListener((val) =>
        {
            if (val) massFacade.Variable = massFacade.VariableY;
        });

        zToggle.onValueChanged.AddListener((val) =>
        {
            if (val) massFacade.Variable = massFacade.VariableZ;
        });

        thresholdToggle.onValueChanged.AddListener((val) =>
        {
            if (val) massFacade.Variable = massFacade.VariableThreshold;
        });

        thresholdField.onValueChanged.AddListener((val) =>
        {
            if (float.TryParse(val.Replace(",","."), out float v))
            {
                massFacade.Threshold = v;
            }
        });

        CheckToggleInteractivy();
        useAuto.onValueChanged.AddListener((val) =>
        {
            CheckToggleInteractivy();
            massFacade.AutoMode = val;
        });
    }

    private void CheckToggleInteractivy()
    {
        xToggle.interactable = !useAuto.isOn;
        yToggle.interactable = !useAuto.isOn;
        zToggle.interactable = !useAuto.isOn;
        thresholdToggle.interactable = !useAuto.isOn;
        thresholdField.interactable = !useAuto.isOn;
        greaterThanToggle.interactable = !useAuto.isOn;
        lessThanToggle.interactable = !useAuto.isOn;
        differenceToggle.interactable = !useAuto.isOn;
    }
}
