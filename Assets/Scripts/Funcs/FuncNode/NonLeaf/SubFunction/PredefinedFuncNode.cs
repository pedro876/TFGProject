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

        protected override float SolveSelf(float[] values)
        {
            switch (functionName)
            {
                case "cos": return Mathf.Cos(values[0]);
                case "sin": return Mathf.Sin(values[0]);
                case "abs": return Mathf.Abs(values[0]);
                default: return 0f;
            }
        }
    }
}