using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class FunctionElement : MonoBehaviour
{
    public bool IsBeingEdit => inputField.isFocused;
    public bool IsSelected => funcFacade.IsFuncSelected(funcName);
    public string FuncName => funcName;

    [SerializeField] Button selectBtn;
    [SerializeField] Image camImage;
    [SerializeField] TMP_InputField inputField;

    private string func;
    private string funcName;
    private FunctionMenu panel;
    private IFuncFacade funcFacade;

    public void Init()
    {
        panel = GetComponentInParent<FunctionMenu>();
        selectBtn.onClick.AddListener(() => OnSelected());
        inputField.onValueChanged.AddListener((str) => UpdateFunction());
        inputField.onDeselect.AddListener((str) => OnEndEdit());
        inputField.onSubmit.AddListener((str) => OnEndEdit());
        inputField.onEndEdit.AddListener((str) => OnEndEdit());
        funcFacade = ServiceLocator.Instance.GetService<IFuncFacade>();
        OnUnselected();
        funcFacade.onChanged += CheckIsUnselected;
    }

    private void OnEndEdit()
    {
        var input = inputField.text.Trim();
        if (input.Equals(""))
        {
            panel.RemoveFunctionElement(this);
        }
        else
        {
            UpdateFunction();
        }
    }

    private void UpdateFunction()
    {
        func = inputField.text.Trim();
        funcName = ExtractFuncName();
        funcFacade.CreateFunc(func);
        func = funcFacade.GetFuncByName(funcName);
    }

    private string ExtractFuncName()
    {
        StringBuilder name = new StringBuilder();
        for(int i = 0; i < func.Length; i++)
        {
            if (func[i] != '(' && func[i] != '=')
            {
                name.Append(func[i]);
            }
            else break;
        }
        return name.ToString();
    }

    public void Focus()
    {
        inputField.Select();
    }

    private void CheckIsUnselected()
    {
        if (!IsSelected && !IsBeingEdit)
        {
            OnUnselected();
        }
    }
    public void SetInput(string input)
    {
        inputField.text = input;
    }

    public void Select()
    {
        OnSelected();
    }
    private void OnSelected()
    {
        funcFacade.SelectFunc(funcName);
        selectBtn.interactable = false;
        camImage.enabled = true;
    }

    private void OnUnselected()
    {
        selectBtn.interactable = true;
        camImage.enabled = false;
        inputField.text = func;
    }
}
