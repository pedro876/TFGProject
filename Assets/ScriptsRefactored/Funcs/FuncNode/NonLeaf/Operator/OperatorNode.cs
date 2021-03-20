using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FuncSpace
{
    public abstract class OperatorNode : NonLeafNode
    {
        private int operatorPriority = -1;
        protected string operatorSymbol;

        public OperatorNode(List<IFuncNode> children, int operatorPriority) : base(children)
        {
            this.operatorPriority = operatorPriority;
        }

        public IFuncNode LeftChild => GetChild(0);
        public IFuncNode RightChild => GetChild(1);

        public override bool NeedsParenthesis()
        {
            if (!HasParent) return false;
            else
            {
                if(Parent is OperatorSubNode)
                {
                    OperatorSubNode parentOpSub = (OperatorSubNode)Parent;
                    if(parentOpSub.RightChild == this)
                    {
                        return true;
                    }
                }
                if (Parent is OperatorNode)
                {
                    OperatorNode parentOp = (OperatorNode)Parent;
                    if (!LeftChild.NeedsRepresentation() || !RightChild.NeedsRepresentation())
                        return false;
                    else
                        return operatorPriority < parentOp.operatorPriority;
                }
                else
                    return false;
            }
        }

        public override void ToStringDeep(StringBuilder builder)
        {
            bool needsParenthesis = NeedsParenthesis();
            if (needsParenthesis) builder.Append('(');
            LeftChild.ToStringDeep(builder);
            builder.Append(operatorSymbol);
            RightChild.ToStringDeep(builder);
            if (needsParenthesis) builder.Append(')');
        }
    }
}