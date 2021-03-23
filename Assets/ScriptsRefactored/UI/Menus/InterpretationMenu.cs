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

    private IFuncFacade funcFacade;
    private IMassFacade massFacade;

    private void Start()
    {
        funcFacade = ServiceLocator.Instance.GetService<IFuncFacade>();
        massFacade = ServiceLocator.Instance.GetService<IMassFacade>();

        GetOriginalData();
        LinkData();

        funcFacade.onChanged += ChooseInterpretation;
    }

    private void GetOriginalData()
    {
        greaterThanToggle.isOn = massFacade.Criterion == massFacade.GreaterCriterion;
        lessThanToggle.isOn = massFacade.Criterion == massFacade.LessCriterion;
        differenceToggle.isOn = massFacade.Criterion == massFacade.MinDifferenceCriterion;

        xToggle.isOn = massFacade.Variable == massFacade.VariableX;
        yToggle.isOn = massFacade.Variable == massFacade.VariableY;
        zToggle.isOn = massFacade.Variable == massFacade.VariableZ;
        thresholdToggle.isOn = massFacade.Variable == massFacade.VariableThreshold;

        differenceVal.text = "" + Mathf.RoundToInt(massFacade.MinDifference * 10f) / 10f;
        differenceSlider.value = massFacade.MinDifference;
        thresholdField.text = "" + massFacade.Threshold;
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
            if (val) massFacade.Variable = massFacade.VariableZ;
        });

        thresholdField.onValueChanged.AddListener((val) =>
        {
            if (float.TryParse(val.Replace(".",","), out float v))
            {
                massFacade.Threshold = v;
            }
        });

        CheckToggleInteractivy();
        useAuto.onValueChanged.AddListener((val) =>
        {
            CheckToggleInteractivy();
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

    private void ChooseInterpretation()
    {
        if (!useAuto.isOn) return;
        
        bool hasX = funcFacade.SelectedFuncUsesVariable("x");
        bool hasY = funcFacade.SelectedFuncUsesVariable("y");
        bool hasZ = funcFacade.SelectedFuncUsesVariable("z");

        massFacade.Criterion = massFacade.GreaterCriterion;

        if (hasX && hasY && hasZ)
            thresholdToggle.isOn = true;
        else if (hasX && hasY)
            zToggle.isOn = true;
        else if(hasY && hasZ)
            xToggle.isOn = true;
        else if (hasX && hasZ)
            yToggle.isOn = true;
        else if (hasX)
            zToggle.isOn = true;
        else if (hasY)
            zToggle.isOn = true;
        else if (hasZ)
            xToggle.isOn = true;
    }
}
