using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuncSpace
{
    public class UserDefinedFuncNode : SubFuncNode
    {
        IFunc func;
        public IFunc Func => func;
        bool preventRecursive;
        public UserDefinedFuncNode(string function, IFunc func, bool preventRecursive, List<IFuncNode> children) : base(children)
        {
            this.functionName = function;
            this.func = func;
            this.preventRecursive = preventRecursive;
        }
    }
}