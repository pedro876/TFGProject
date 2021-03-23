using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FunctionElement : MonoBehaviour
{
    public static FunctionElement selectedFunc;

    [SerializeField] Button selectBtn;
    [SerializeField] Image camImage;
    [SerializeField] TMP_InputField inputField;
    public FunctionC func;
    FunctionMenu panel;

    public bool isBeingEdit { get => inputField.isFocused; }
    public static bool HasValidFunc { get => selectedFunc != null && selectedFunc.func != null; }

    private void Awake()
    {
        panel = GetComponentInParent<FunctionMenu>();
        selectBtn.onClick.AddListener(() => panel.SelectFunction(this));
        camImage.enabled = selectedFunc == this;
        inputField.onValueChanged.AddListener((str)=>UpdateFunction());
        inputField.onDeselect.AddListener((str)=>OnEndEdit());
        inputField.onSubmit.AddListener((str) => OnEndEdit());
        inputField.onEndEdit.AddListener((str) => OnEndEdit());
    }

    private void OnEndEdit()
    {
        if (inputField.text.Trim() == "")
        {
            panel.RemoveFunctionElement(this);
        } else if (func != null) inputField.text = func.ToString();
    }

    public void Focus()
    {
        inputField.Select();
    }

    public void SetFunction(FunctionC function)
    {
        if (func != null && !func.Equals(function))
        {
            func = function;
            FunctionMenu.OnChanged();
        } else func = function;
        if (!isBeingEdit)
        {
            inputField.onValueChanged.RemoveAllListeners();
            inputField.text = func.ToString();
            inputField.onValueChanged.AddListener((str) => UpdateFunction());
        }
        
    }

    public void UpdateFunction()
    {
        FunctionC f = FunctionManager.AddFunction(inputField.text);
        if(f != null)
        {
            SetFunction(f);
        }
    }

    public void OnSelected()
    {
        selectedFunc = this;
        selectBtn.interactable = false;
        camImage.enabled = true;
    }

    public void OnUnselected()
    {
        selectBtn.interactable = true;
        camImage.enabled = false;
        if (func != null) inputField.text = func.ToString();
    }
}
