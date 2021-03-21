using FuncSpace;
using UnityEngine;

public class FuncFacade : IFuncFacade
{

    IFuncFactory factory;
    IFuncSolver funcSolver;
    IBytecodeSolver bytecodeSolver;

    public FuncFacade()
    {
        factory = new FuncFactory();
        funcSolver = new FuncSolver();
        bytecodeSolver = new BytecodeSolver();
    }

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

    public float Solve(Vector3 vec)
    {
        return funcSolver.Solve(vec, selectedFunc);
    }

    public float SolveBytecode(Vector3 vec, float[] memory = null)
    {
        if (memory == null)
            memory = selectedFunc.BytecodeInfo.memory;
        return bytecodeSolver.Solve(vec, selectedFunc.BytecodeInfo, memory);
    }

    public float[] GetBytecodeMemCopy()
    {
        float[] copy = new float[Bytecode.maxMemorySize];
        for (int i = 0; i < Bytecode.maxMemorySize; i++)
            copy[i] = selectedFunc.BytecodeInfo.memory[i];
        return copy;
    }

    #endregion
}
