using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuncFacade
{
    private static FuncFacade instance = null;
    public static FuncFacade Instance
    {
        get
        {
            if (instance == null)
                instance = new FuncFacade();
            return instance; ;
        }
    }

    private FuncSpace.FuncFactory factory;
    private FuncSpace.IFunc selectedFunc;

    public FuncFacade()
    {
        factory = FuncSpace.FuncFactory.Instance;
        selectedFunc = factory.DummyFunc;
    }
}
