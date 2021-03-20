using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FuncSpace
{
    public interface IFuncReader
    {
        IFunc ExtractOriginalFuncInfo(string textFunc, IFunc func);
        IFunc ExtractFinalFuncInfo(IFunc func);
    }
}