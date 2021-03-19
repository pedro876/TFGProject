﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuncSpace
{
    public class OperatorPowNode : OperatorNode
    {
        public OperatorPowNode(List<IFuncNode> children) : base(children, 1) { }

        protected override float SolveSelf(float[] values)
        {
            return Mathf.Pow(values[0], values[1]);
        }
    }
}