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
        protected override float SolveSelf(float[] values)
        {
            return values[0] + values[1];
        }
    }
}