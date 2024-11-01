﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class FunctionMenu : MonoBehaviour
{
    [SerializeField] float removeTime = 3f;
    [SerializeField] float removeDuplicatesTime = 0.5f;

    [SerializeField] GameObject elemPrefab;
    [SerializeField] Transform elemParent;
    [SerializeField] Button addBtn;
    [SerializeField] Transform addElemTransform;
    
    private List<FunctionElement> allFuncElements;

    private IFuncFacade funcFacade;


    private void Awake()
    {
        allFuncElements = new List<FunctionElement>();
        addBtn.onClick.AddListener(() => AddFunctionElement(true));
    }

    private void Start()
    {
        funcFacade = ServiceLocator.Instance.GetService<IFuncFacade>();
        AddPredefinedFunctions();
    }

    private void AddPredefinedFunctions()
    {
        var allDefinedFuncs = funcFacade.GetAllFuncNames();
        foreach (var funcName in allDefinedFuncs)
        {
            var elem = AddFunctionElement();
            var func = funcFacade.GetFuncByName(funcName);
            elem.SetInput(func);
        }
        addElemTransform.SetAsLastSibling();
    }

    private void OnEnable()
    {
        StartCoroutine(RemoveUnusedFunctions());
        StartCoroutine(RemoveDuplicates());
    }

    IEnumerator RemoveUnusedFunctions()
    {
        while (true)
        {
            yield return new WaitForSeconds(removeTime);
            var allDefinedFuncs = funcFacade.GetAllFuncNames();
            foreach(var funcName in allDefinedFuncs)
            {
                bool hasFunctionElement = false;
                foreach(var elem in allFuncElements)
                {
                    if (elem.FuncName.Equals(funcName))
                    {
                        hasFunctionElement = true;
                    }
                }
                if (!hasFunctionElement)
                {
                    funcFacade.RemoveFunc(funcName);
                }
            }
        }
    }

    IEnumerator RemoveDuplicates()
    {
        while (true)
        {
            yield return new WaitForSeconds(removeDuplicatesTime);
            var namesMap = new Dictionary<string, FunctionElement>();
            var toRemove = new List<FunctionElement>();
            foreach(var elem in allFuncElements)
            {
                if (namesMap.ContainsKey(elem.FuncName))
                    toRemove.Add(elem);
                else
                    namesMap.Add(elem.FuncName, elem);
            }
            foreach(var elem in toRemove)
            {
                RemoveFunctionElement(elem);
            }
        }
    }

    public FunctionElement AddFunctionElement(bool focus = false)
    {
        FunctionElement elem = Instantiate(elemPrefab, elemParent).GetComponent<FunctionElement>();
        elem.Init();
        if(focus) elem.Focus();
        elem.name = "func " + allFuncElements.Count;
        allFuncElements.Add(elem);
        if (allFuncElements.Count == 1) elem.Select();
        addElemTransform.SetAsLastSibling();
        return elem;
    }

    public void RemoveFunctionElement(FunctionElement elem)
    {
        allFuncElements.Remove(elem);
        if(elem.IsSelected && allFuncElements.Count > 0)
        {
            elem.Select();
        }
        Destroy(elem.gameObject);

        if(allFuncElements.Count == 1)
        {
            allFuncElements[0].Select();
        }
    }
}
