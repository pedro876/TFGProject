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
        greaterThanToggle.isOn = VolumeInterpreter.criterion == VolumeInterpreter.Criterion.greaterThan;
        lessThanToggle.isOn = VolumeInterpreter.criterion == VolumeInterpreter.Criterion.lessThan;

        xToggle.isOn = VolumeInterpreter.variable == VolumeInterpreter.Variable.x;
        yToggle.isOn = VolumeInterpreter.variable == VolumeInterpreter.Variable.y;
        zToggle.isOn = VolumeInterpreter.variable == VolumeInterpreter.Variable.z;
        thresholdToggle.isOn = VolumeInterpreter.variable == VolumeInterpreter.Variable.threshold;

        thresholdField.text = "" + VolumeInterpreter.threshold;

        greaterThanToggle.onValueChanged.AddListener((val) =>
        {
            if (val) VolumeInterpreter.SetCriterion(VolumeInterpreter.Criterion.greaterThan);
        });
        lessThanToggle.onValueChanged.AddListener((val) =>
        {
            if (val) VolumeInterpreter.SetCriterion(VolumeInterpreter.Criterion.lessThan);
        });
        xToggle.onValueChanged.AddListener((val) =>
        {
            if (val) VolumeInterpreter.SetVariable(VolumeInterpreter.Variable.x);
        });
        yToggle.onValueChanged.AddListener((val) =>
        {
            if (val) VolumeInterpreter.SetVariable(VolumeInterpreter.Variable.y);
        });
        zToggle.onValueChanged.AddListener((val) =>
        {
            if (val) VolumeInterpreter.SetVariable(VolumeInterpreter.Variable.z);
        });
        thresholdToggle.onValueChanged.AddListener((val) =>
        {
            if (val) VolumeInterpreter.SetVariable(VolumeInterpreter.Variable.threshold);
        });
        thresholdField.onValueChanged.AddListener((val) =>
        {
            float v;
            val = val.Replace(".", ",");
            if(float.TryParse(val, out v))
            {
                VolumeInterpreter.SetThreshold(v);
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
        if (FunctionElement.selectedFunc == null || FunctionElement.selectedFunc.func == null) return;

        Function func = FunctionElement.selectedFunc.func;
        bool hasX = func.variables.Contains("x");
        bool hasY = func.variables.Contains("y");
        bool hasZ = func.variables.Contains("z");

        VolumeInterpreter.criterion = VolumeInterpreter.Criterion.greaterThan;

        if (hasX && hasY && hasZ)
        {
            thresholdToggle.isOn = true;
        }
        else if (hasX && hasY)
        {
            zToggle.isOn = true;
        }
        else if(hasY && hasZ)
        {
            xToggle.isOn = true;
        }
        else if (hasX && hasZ)
        {
            yToggle.isOn = true;
        }
        else if (hasX)
        {
            zToggle.isOn = true;
        }
        else if (hasY)
        {
            zToggle.isOn = true;
        }
        else if (hasZ)
        {
            xToggle.isOn = true;
        }
    }
}
