using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FuncSpace
{
    public abstract class SubFuncNode : NonLeafNode
    {
        protected string functionName;
        public SubFuncNode(List<IFuncNode> children) : base(children) { }

        public override bool NeedsParenthesis()
        {
            return false;
        }

        public override void ToStringDeep(StringBuilder builder)
        {
            builder.Append(functionName);
            builder.Append('(');
            int childrenCount = GetChildren().Count;
            for(int i = 0; i < childrenCount; i++)
            {
                GetChild(i).ToStringDeep(builder);
                if (i < childrenCount - 1) builder.Append(',');
            }
            builder.Append(')');
        }
    }
}