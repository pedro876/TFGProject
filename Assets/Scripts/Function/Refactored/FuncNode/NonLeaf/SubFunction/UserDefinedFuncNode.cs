using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuncSpace
{
    public class UserDefinedFuncNode : SubFuncNode
    {
        IFunc func;
        bool preventRecursive;
        public UserDefinedFuncNode(string function, IFunc func, bool preventRecursive, List<IFuncNode> children) : base(children)
        {
            this.functionName = function;
            this.func = func;
            this.preventRecursive = preventRecursive;
        }

        protected override float SolveSelf(float[] values)
        {
            if (preventRecursive)
            {
                return 1f;
            } else
            {
                return func.Solve(values[0], values[1], values[2]);
            }
        }
    }
}