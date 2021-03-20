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
        protected override float SolveSelf(float[] values)
        {
            return values[0] / values[1];
        }
    }
}