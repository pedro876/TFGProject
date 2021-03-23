using System;

namespace FuncSpace
{
    public interface IFuncFactory
    {
        IFunc GetDummy();
        void ForEachFuncName(Action<string> action);
        void ForEachUserDefinedFuncName(Action<string> action);
        int IndexOfPredefinedFunc(string funcName);
        bool IsFuncDefinedByUser(string name);
        IFunc CreateFunc(string textFunc);
        IFunc GetFunc(string name);
        void RemoveAllFuncs();
        void RemoveFunc(string name);
    }
}