using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuncSpace
{
    public class PredefinedFuncNode : SubFuncNode
    {
        public PredefinedFuncNode(string function, List<IFuncNode> children) : base(children)
        {
            this.functionName = function;
        }
    }
}