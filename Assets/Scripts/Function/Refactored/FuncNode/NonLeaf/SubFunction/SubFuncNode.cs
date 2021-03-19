using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuncSpace
{
    public abstract class SubFuncNode : NonLeafNode
    {
        public SubFuncNode(List<IFuncNode> children) : base(children) { }

        public override bool NeedsParenthesis()
        {
            return false;
        }
    }
}