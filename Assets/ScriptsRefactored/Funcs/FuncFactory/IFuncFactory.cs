using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuncSpace
{
    public interface IFuncFactory
    {
        IFunc DummyFunc { get; }
        int MaxOperatorIndex { get; }
        HashSet<string> AllFuncNames { get; }
        HashSet<string> Variables { get; }
        List<string> Operators { get; }
        List<string> PredefinedFuncs { get; }
        int GetOperatorPriority(string op);
        bool IsFuncDefinedByUser(string name);
        IFunc CreateFunc(string textFunc);
        IFunc GetFunc(string name);
        void RemoveAllFuncs();
        void RemoveFunc(string name);
    }
}