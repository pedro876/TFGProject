using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuncSpace
{
    public interface IFuncInterpreter
    {
        void CreateNodeTreeForFunc(IFunc func);

    }
}