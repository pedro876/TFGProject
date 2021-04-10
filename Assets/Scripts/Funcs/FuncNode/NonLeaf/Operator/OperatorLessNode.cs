using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuncSpace
{
    public class OperatorLessNode : OperatorNode
    {
        public OperatorLessNode(List<IFuncNode> children) : base(children, 2)
        {
            operatorSymbol = "<";
        }
    }
}