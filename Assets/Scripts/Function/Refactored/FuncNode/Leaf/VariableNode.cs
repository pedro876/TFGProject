using System.Collections;
using UnityEngine;

namespace FuncSpace
{
    public class VariableNode : LeafNode
    {
        private string variable;
        private bool positive;
        public bool IsPositive { get; set; }
        public VariableNode(string variable)
        {
            this.variable = variable;
            positive = true;
        }

        public override float Solve(float x, float y, float z)
        {
            switch (variable)
            {
                case "x": return x;
                case "y": return y;
                case "z": return z;
                default: return 0f;
            }
        }

        public override bool NeedsRepresentation()
        {
            return true;
        }

        public override bool NeedsParenthesis()
        {
            if (!HasParent()) return false;
            else return !positive;
            /*else
            {
                IFuncNode parent = GetParent();
                if(parent is OperatorSubNode)
                {
                    OperatorSubNode parentOp = (OperatorSubNode)parent;
                    if(parentOp.GetChildRight() == this)
                    {
                        return !positive;
                    }
                }
                return !positive;
            }*/

        }
    }
}