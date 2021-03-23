using System.Collections.Generic;
using System;
using UnityEngine;

public interface IFuncFacade
{
    List<string> GetAllFuncNames();
    string GetFuncByName(string name);
    string GetSelectedFunc();
    string GetSelectedFuncName();
    bool SelectFunc(string funcName);
    bool IsFuncSelected(string funcName);
    bool SelectedFuncUsesVariable(string variable);

    void CreateFunc(string textFunc);
    bool RemoveFunc(string funcName);
    void Reset();

    float Solve(ref Vector3 pos);
    float Solve(ref Vector3 pos, float[] memory);

    float[] GetBytecodeMemCopy();
    List<int> GetBytecodeOperations();
    int GetBytecodeResultIndex();
    int GetMaxOperatorIndex();
    int GetMaxMemorySize();
    int GetMaxOperationsSize();

    event Action onChanged;
}
