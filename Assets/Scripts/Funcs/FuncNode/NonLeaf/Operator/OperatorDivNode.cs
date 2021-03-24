using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuncSpace
{
    public class OperatorDivNode : OperatorNode
    {
        public OperatorDivNode(List<IFuncNode> children) : base(children, 1)
        {
            operatorSymbol = "/";
        }
    }
}