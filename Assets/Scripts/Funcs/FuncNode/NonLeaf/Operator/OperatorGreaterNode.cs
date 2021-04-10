using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuncSpace
{
    public class OperatorGreaterNode : OperatorNode
    {
        public OperatorGreaterNode(List<IFuncNode> children) : base(children, 2)
        {
            operatorSymbol = ">";
        }
    }
}