using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuncSpace
{
    public class OperatorSubNode : OperatorNode
    {
        public OperatorSubNode(List<IFuncNode> children) : base(children, 0)
        {
            operatorSymbol = "-";
        }
    }
}