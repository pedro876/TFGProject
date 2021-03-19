using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class FunctionPanel : MonoBehaviour
{
    //[SerializeField] float updateTime = 0.2f;
    [SerializeField] float removeTime = 3f;

    List<FunctionElement> allFunctions;
    [SerializeField] GameObject elemPrefab;
    [SerializeField] Transform elemParent;

    [SerializeField] Button addBtn;
    [SerializeField] Transform addElement;

    public static event Action onPreChanged;
    public static event Action onChanged;

    private void Start()
    {
        allFunctions = new List<FunctionElement>();
        FunctionManager.AddFunction("f(x) = cos(x) + cos(y)");
        FunctionManager.AddFunction("g(x)=x^2");
        FunctionManager.AddFunction("t(x)=x");
        FunctionElement elem;
        foreach(Function f in FunctionManager.functions.Values)
        {
            elem = AddFunctionElement();
            elem.SetFunction(f);
        }
        addBtn.onClick.AddListener(() => AddFunctionElement(true));
        addElement.SetAsLastSibling();
    }

    public static void OnChanged()
    {
        if(FunctionElement.selectedFunc != null && FunctionElement.selectedFunc.func != null)
        {
            onPreChanged?.Invoke();
            onChanged?.Invoke();
        }
    }

    private void OnEnable()
    {
        StartCoroutine(RemoveUnusedFunctions());
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
        func.UpdateFunction();
        foreach(FunctionElement f in allFunctions)
        {
            if (f != func) f.OnUnselected();
        }
        OnChanged();
    }
}
