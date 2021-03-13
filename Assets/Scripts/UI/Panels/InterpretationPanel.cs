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
    [SerializeField] TMP_InputField thresholdField;

    [SerializeField] Toggle useAuto;

    private void Start()
    {
        greaterThanToggle.isOn = VolumeInterpreter.Criterion == VolumeInterpreter.CriterionType.GreaterThan;
        lessThanToggle.isOn = VolumeInterpreter.Criterion == VolumeInterpreter.CriterionType.LessThan;

        xToggle.isOn = VolumeInterpreter.Variable == VolumeInterpreter.VariableType.X;
        yToggle.isOn = VolumeInterpreter.Variable == VolumeInterpreter.VariableType.Y;
        zToggle.isOn = VolumeInterpreter.Variable == VolumeInterpreter.VariableType.Z;
        thresholdToggle.isOn = VolumeInterpreter.Variable == VolumeInterpreter.VariableType.Threshold;

        thresholdField.text = "" + VolumeInterpreter.Threshold;

        greaterThanToggle.onValueChanged.AddListener((val) =>
        {
            if (val) VolumeInterpreter.Criterion = VolumeInterpreter.CriterionType.GreaterThan;
        });

        lessThanToggle.onValueChanged.AddListener((val) =>
        {
            if (val) VolumeInterpreter.Criterion= VolumeInterpreter.CriterionType.LessThan;
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

        useAuto.onValueChanged.AddListener((val) =>
        {
            xToggle.interactable = !val;
            yToggle.interactable = !val;
            zToggle.interactable = !val;
            thresholdToggle.interactable = !val;
            thresholdField.interactable = !val;
            greaterThanToggle.interactable = !val;
            lessThanToggle.interactable = !val;
        });

        RendererManager.renderStarted += ChooseInterpretation;
    }

    private void ChooseInterpretation()
    {
        if (!useAuto.isOn) return;
        if (!FunctionElement.hasValidFunc) return;

        Function func = FunctionElement.selectedFunc.func;
        bool hasX = func.variables.Contains("x");
        bool hasY = func.variables.Contains("y");
        bool hasZ = func.variables.Contains("z");

        VolumeInterpreter.Criterion = VolumeInterpreter.CriterionType.GreaterThan;

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
