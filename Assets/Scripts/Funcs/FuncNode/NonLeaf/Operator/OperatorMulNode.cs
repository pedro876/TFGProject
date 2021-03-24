using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuncSpace
{
    public class OperatorMulNode : OperatorNode
    {
        public OperatorMulNode(List<IFuncNode> children) : base(children, 1)
        {
            operatorSymbol = "*";
        }
    }
}