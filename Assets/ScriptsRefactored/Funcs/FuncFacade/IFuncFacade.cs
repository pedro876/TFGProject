using UnityEngine;

public interface IFuncFacade
{
    string SelectedFunc { get; }
    string SelectedFuncName { get; }
    bool SelectFunc(string funcName);
    bool IsFuncSelected(string funcName);
    void CreateFunc(string textFunc);
    bool RemoveFunc(string funcName);
    void Reset();
    float Solve(Vector3 vec);
}
