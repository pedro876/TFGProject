using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FunctionElement : MonoBehaviour
{
    public static FunctionElement selectedFunc;

    [SerializeField] Button selectBtn;
    FunctionPanel panel;
    [SerializeField] Image camImage;
    public Function func;
    [SerializeField] TMP_InputField inputField;

    private void Awake()
    {
        panel = GetComponentInParent<FunctionPanel>();
        selectBtn.onClick.AddListener(() => panel.SelectFunction(this));
        camImage.enabled = false;
    }

    private void Update()
    {
        if(inputField.text.Trim() == "" && !IsBeingEdit())
        {
            panel.RemoveFunctionElement(this);
        }
    }

    public void Focus()
    {
        inputField.Select();
    }

    public bool IsBeingEdit()
    {
        return inputField.isFocused;
    }

    public void SetFunction(Function function)
    {
        func = function;
        if (!IsBeingEdit())
        {
            inputField.text = func.ToString();
        }
        
    }

    public void UpdateFunction()
    {
        Function f = FunctionManager.AddFunction(inputField.text);
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
    }
}
