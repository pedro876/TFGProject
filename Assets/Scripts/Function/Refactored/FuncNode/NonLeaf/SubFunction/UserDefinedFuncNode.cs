using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuncSpace
{
    public class UserDefinedFuncNode : SubFuncNode
    {
        string function;
        public UserDefinedFuncNode(string function, List<IFuncNode> children) : base(children)
        {
            this.function = function;
        }

        protected override float SolveSelf(float[] values)
        {
            switch (function)
            {
                case "cos": return Mathf.Cos(values[0]);
                case "sin": return Mathf.Sin(values[0]);
                case "abs": return Mathf.Abs(values[0]);
                default: return 0f;
            }
        }
    }
}