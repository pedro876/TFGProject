using FuncSpace;

public class FuncFacade : IFuncFacade
{
    /*#region SetUp

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

    private void Init()
    {
        factory = FuncSpace.FuncFactory.Instance;
        selectedFunc = factory.DummyFunc;
    }

    #endregion*/

    FuncFactory factory;
    public FuncFacade()
    {
        factory = new FuncFactory();
    }

    /*private FuncSpace.FuncFactory factory;*/
    private FuncSpace.IFunc selectedFunc;

    #region selection

    public string SelectedFunc => selectedFunc.ToString();
    public string SelectedFuncName => selectedFunc.Name;

    public bool SelectFunc(string funcName)
    {
        if (factory.IsFuncDefinedByUser(funcName))
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
        if (factory.IsFuncDefinedByUser(funcName))
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
