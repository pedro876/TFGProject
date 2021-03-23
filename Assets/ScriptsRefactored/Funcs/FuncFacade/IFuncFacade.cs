using System.Collections.Generic;
using System;
public interface IFuncFacade
{
    string GetSelectedFunc();
    string GetSelectedFuncName();
    bool SelectFunc(string funcName);
    bool IsFuncSelected(string funcName);
    bool SelectedFuncUsesVariable(string variable);

    void CreateFunc(string textFunc);
    bool RemoveFunc(string funcName);
    void Reset();

    float Solve(float x, float y, float z);
    float Solve(float x, float y, float z, float[] memory);

    float[] GetBytecodeMemCopy();
    List<int> GetBytecodeOperations();
    int GetBytecodeResultIndex();
    int GetMaxOperatorIndex();
    event Action onChanged;
}
