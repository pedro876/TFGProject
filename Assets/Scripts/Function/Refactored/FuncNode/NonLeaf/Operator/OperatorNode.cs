using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuncSpace
{
    public abstract class OperatorNode : NonLeafNode
    {
        private int operatorPriority = -1;

        public OperatorNode(List<IFuncNode> children, int operatorPriority) : base(children)
        {
            this.operatorPriority = operatorPriority;
        }

        public IFuncNode GetChildLeft()
        {
            return GetChild(0);
        }
        public IFuncNode GetChildRight()
        {
            return GetChild(1);
        }

        public override bool NeedsParenthesis()
        {
            if (!HasParent()) return false;
            else
            {
                IFuncNode parent = GetParent();
                if(parent is OperatorSubNode)
                {
                    OperatorSubNode parentOpSub = (OperatorSubNode)parent;
                    if(parentOpSub.GetChildRight() == this)
                    {
                        return true;
                    }
                }
                if (parent is OperatorNode)
                {
                    OperatorNode parentOp = (OperatorNode)parent;
                    return operatorPriority < parentOp.operatorPriority;
                }
                else
                    return false;
            }
        }
    }
}