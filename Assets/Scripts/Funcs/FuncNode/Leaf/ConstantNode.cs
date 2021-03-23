using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FuncSpace
{
    public class ConstantNode : LeafNode
    {
        private static Dictionary<string, float> symbolValues = new Dictionary<string, float>()
        {
            { "pi", Mathf.PI },
            { "e", Mathf.Exp(1) },
            {"inf", Mathf.Infinity },
        };

        private float value;
        private bool usesSymbol;
        private string symbol;

        public ConstantNode(float value)
        {
            this.value = value;
            this.usesSymbol = false;
            this.symbol = "";
        }

        public ConstantNode(string symbol)
        {
            this.symbol = symbol.ToLower();
            this.usesSymbol = symbolValues.ContainsKey(this.symbol);
            if (usesSymbol)
            {
                value = symbolValues[this.symbol];
            } else if(float.TryParse(symbol.Replace(".",","), out float result))
            {
                value = result;
            }
            else value = 1f;
        }

        public float GetValue()
        {
            return value;
        }

        public void NegateValue()
        {
            value = -Mathf.Abs(value);
        }

        public override float Solve(float x, float y, float z)
        {
            return value;
        }

        public override bool NeedsRepresentation()
        {
            if(value == 0f)
            {
                if (Parent is OperatorAddNode || Parent is OperatorSubNode)
                {
                    return false;
                }
            }
            return true;
        }

        public override bool NeedsParenthesis()
        {
            if (!HasParent) return false;
            else return value < 0f;
            /*else
            {
                IFuncNode parent = GetParent();
                if (parent is OperatorSubNode)
                {
                    OperatorSubNode parentOp = (OperatorSubNode)parent;
                    if (parentOp.GetChildRight() == this)
                    {
                        return value < 0f;
                    }
                }
                return value < 0f;
            }*/
        }

        public override void ToStringDeep(StringBuilder builder)
        {
            if (NeedsRepresentation())
            {
                bool needsParenthesis = NeedsParenthesis();
                if (needsParenthesis) builder.Append('(');
                builder.Append(value.ToString());
                if (needsParenthesis) builder.Append(')');
            }
        }
    }
}