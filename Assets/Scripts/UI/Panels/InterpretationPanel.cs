using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InterpretationPanel : MonoBehaviour
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

    private void Awake()
    {
        greaterThanToggle.isOn = VolumeInterpreter.Criterion == VolumeInterpreter.CriterionType.Greater;
        lessThanToggle.isOn = VolumeInterpreter.Criterion == VolumeInterpreter.CriterionType.Less;
        differenceToggle.isOn = VolumeInterpreter.Criterion == VolumeInterpreter.CriterionType.MinDifference;

        xToggle.isOn = VolumeInterpreter.Variable == VolumeInterpreter.VariableType.X;
        yToggle.isOn = VolumeInterpreter.Variable == VolumeInterpreter.VariableType.Y;
        zToggle.isOn = VolumeInterpreter.Variable == VolumeInterpreter.VariableType.Z;
        thresholdToggle.isOn = VolumeInterpreter.Variable == VolumeInterpreter.VariableType.Threshold;

        differenceVal.text = "" + Mathf.RoundToInt(VolumeInterpreter.MinDifference * 10f) / 10f;
        differenceSlider.value = VolumeInterpreter.MinDifference;
        thresholdField.text = "" + VolumeInterpreter.Threshold;

        greaterThanToggle.onValueChanged.AddListener((val) =>
        {
            if (val) VolumeInterpreter.Criterion = VolumeInterpreter.CriterionType.Greater;
        });

        lessThanToggle.onValueChanged.AddListener((val) =>
        {
            if (val) VolumeInterpreter.Criterion= VolumeInterpreter.CriterionType.Less;
        });

        differenceToggle.onValueChanged.AddListener((val) =>
        {
            if (val) VolumeInterpreter.Criterion = VolumeInterpreter.CriterionType.MinDifference;
        });
        differenceSlider.onValueChanged.AddListener((val) =>
        {
            if (differenceToggle.interactable)
            {
                VolumeInterpreter.MinDifference = val;
            }
            differenceVal.text = "" + Mathf.RoundToInt(val * 10f) / 10f;
        });

        xToggle.onValueChanged.AddListener((val) =>
        {
            if (val) VolumeInterpreter.Variable= VolumeInterpreter.VariableType.X;
        });

        yToggle.onValueChanged.AddListener((val) =>
        {
            if (val) VolumeInterpreter.Variable= VolumeInterpreter.VariableType.Y;
        });

        zToggle.onValueChanged.AddListener((val) =>
        {
            if (val) VolumeInterpreter.Variable= VolumeInterpreter.VariableType.Z;
        });

        thresholdToggle.onValueChanged.AddListener((val) =>
        {
            if (val) VolumeInterpreter.Variable= VolumeInterpreter.VariableType.Threshold;
        });

        thresholdField.onValueChanged.AddListener((val) =>
        {
            float v;
            val = val.Replace(".", ",");
            if(float.TryParse(val, out v))
            {
                VolumeInterpreter.Threshold = v;
            }
        });

        xToggle.interactable = !useAuto.isOn;
        yToggle.interactable = !useAuto.isOn;
        zToggle.interactable = !useAuto.isOn;
        thresholdToggle.interactable = !useAuto.isOn;
        thresholdField.interactable = !useAuto.isOn;
        greaterThanToggle.interactable = !useAuto.isOn;
        lessThanToggle.interactable = !useAuto.isOn;
        differenceToggle.interactable = !useAuto.isOn;

        useAuto.onValueChanged.AddListener((val) =>
        {
            xToggle.interactable = !val;
            yToggle.interactable = !val;
            zToggle.interactable = !val;
            thresholdToggle.interactable = !val;
            thresholdField.interactable = !val;
            greaterThanToggle.interactable = !val;
            lessThanToggle.interactable = !val;
            differenceToggle.interactable = !val;

            if (!val)
            {
                VolumeInterpreter.MinDifference = differenceSlider.value;
            }
        });

        RendererManager.renderStarted += ChooseInterpretation;
    }

    private void ChooseInterpretation()
    {
        if (!useAuto.isOn) return;
        if (!FunctionElement.HasValidFunc) return;

        Function func = FunctionElement.selectedFunc.func;
        bool hasX = func.variables.Contains("x");
        bool hasY = func.variables.Contains("y");
        bool hasZ = func.variables.Contains("z");

        VolumeInterpreter.Criterion = VolumeInterpreter.CriterionType.Greater;

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
