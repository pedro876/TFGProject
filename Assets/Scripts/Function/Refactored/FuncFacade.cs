using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuncFacade
{
    #region SetUp

    private static FuncFacade instance = null;
    public static FuncFacade Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new FuncFacade();
                instance.Init();
            }
                
            return instance;
        }
    }

    private FuncSpace.FuncFactory factory;
    private FuncSpace.IFunc selectedFunc;

    private void Init()
    {
        factory = FuncSpace.FuncFactory.Instance;
        selectedFunc = factory.DummyFunc;
    }

    #endregion

    #region selection

    public string SelectedFunc => selectedFunc.ToString();
    public string SelectedFuncName => selectedFunc.Name;

    public bool SelectFunc(string funcName)
    {
        if (factory.ContainsFunc(funcName))
        {
            selectedFunc = factory.GetFunc(funcName);
            return true;
        }
        else
            return false;
    }

    public bool IsFuncSelected(string funcName)
    {
        return selectedFunc.Name.Equals(funcName);
    }

    #endregion

    #region creation and removal

    public void CreateFunc(string textFunc)
    {
        factory.CreateFunc(textFunc);
    }

    public bool RemoveFunc(string funcName)
    {
        if (factory.ContainsFunc(funcName))
        {
            var func = factory.GetFunc(funcName);
            if (selectedFunc == func)
                selectedFunc = factory.DummyFunc;
            factory.RemoveFunc(funcName);
            return true;
        }
        else
            return false;
    }

    public void Reset()
    {
        factory.RemoveAllFuncs();
        selectedFunc = factory.DummyFunc;
    }

    #endregion

    #region Solve

    public float Solve(float x, float y, float z)
    {
        return selectedFunc.Solve(x, y, z);
    }

    #endregion
}
