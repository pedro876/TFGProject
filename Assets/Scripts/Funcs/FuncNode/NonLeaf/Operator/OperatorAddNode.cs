using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuncSpace
{
    public class OperatorAddNode : OperatorNode
    {
        public OperatorAddNode(List<IFuncNode> children) : base(children, 0)
        {
            operatorSymbol = "+";
        }
    }
}