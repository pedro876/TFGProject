using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class FunctionPanel : MonoBehaviour
{
    [SerializeField] float updateTime = 0.2f;
    [SerializeField] float removeTime = 3f;

    List<FunctionElement> allFunctions;
    [SerializeField] GameObject elemPrefab;
    [SerializeField] Transform elemParent;

    [SerializeField] Button addBtn;
    [SerializeField] Transform addElement;

    public static event Action onChanged;

    private void Start()
    {
        allFunctions = new List<FunctionElement>();
        FunctionManager.AddFunction("f(x)=x");
        FunctionElement elem;
        foreach(Function f in FunctionManager.functions.Values)
        {
            elem = AddFunctionElement();
            elem.SetFunction(f);
        }
        addBtn.onClick.AddListener(() => AddFunctionElement(true));
        addElement.SetAsLastSibling();
    }

    public static void InvokeOnChanged()
    {
        onChanged?.Invoke();
    }

    private void OnEnable()
    {
        StartCoroutine(UpdateFunction());
        StartCoroutine(RemoveUnusedFunctions());
    }

    IEnumerator UpdateFunction()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateTime);
            FunctionElement.selectedFunc?.UpdateFunction();
            //Debug.Log(FunctionElement.selectedFunc.func.ToString());
        }
    }

    IEnumerator RemoveUnusedFunctions()
    {
        while (true)
        {
            yield return new WaitForSeconds(removeTime);
            List<string> toRemove = new List<string>();
            foreach(string func in FunctionManager.functions.Keys)
            {
                bool used = false;
                foreach(var elem in allFunctions)
                {
                    if (elem.func != null && elem.func.name == func) used = true;
                }
                if (!used)
                {
                    Debug.Log("to remove: " + func);
                    toRemove.Add(func);
                }
            }
            foreach (var s in toRemove) FunctionManager.functions.Remove(s);
        }
    }

    public FunctionElement AddFunctionElement(bool focus = false)
    {
        FunctionElement elem = Instantiate(elemPrefab, elemParent).GetComponent<FunctionElement>();
        if(focus) elem.Focus();
        elem.name = "func " + allFunctions.Count;
        allFunctions.Add(elem);
        if (allFunctions.Count == 1) SelectFunction(elem);
        addElement.SetAsLastSibling();
        return elem;
    }

    public void RemoveFunctionElement(FunctionElement elem)
    {
        allFunctions.Remove(elem);
        if(FunctionElement.selectedFunc == elem && allFunctions.Count > 0)
        {
            SelectFunction(allFunctions[0]);
        }
        Destroy(elem.gameObject);
    }

    public void SelectFunction(FunctionElement func)
    {
        func.OnSelected();
        onChanged?.Invoke();
        foreach(FunctionElement f in allFunctions)
        {
            if (f != func) f.OnUnselected();
        }
    }

    //[SerializeField] TMP_InputField inputField;
    //[SerializeField] TextMeshProUGUI displayText;

    /*void Start()
    {
        StartCoroutine(UpdateFunction());
    }*/

    /*IEnumerator UpdateFunction()
    {
        while (true)
        {
            if(inputField != null && displayText != null)
            {
                if (inputField.text.Contains("="))
                {
                    Function f = FunctionManager.AddFunction(inputField.text);

                    //Fix declaration and cursor location
                    string[] parts = inputField.text.Split('=');
                    int displacement = f.declaration.Length + 1 - parts[0].Length;
                    inputField.text = f.declaration + " =" + parts[1];
                    if (displacement > 0)
                    {
                        inputField.selectionAnchorPosition += displacement;
                        inputField.selectionFocusPosition += displacement;
                    }

                    //Display processed function
                    displayText.text = f.ToString();
                }
            }

            yield return new WaitForSeconds(0.2f);
        }
    }*/
}
