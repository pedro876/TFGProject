using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionNode
{
    public FunctionNode parent;
    public FunctionNode childLeft;
    public FunctionNode childRight;
    public bool isOperator;
    //public int operation;
    public float value;
    public bool isVariable;
    public int variable;
    public bool variablePositive = true;
    public bool woreParenthesis = false;
    public int parenthesisLevel = 0;

    public enum Operations
    {
        Sub,
        Add,
        Mult,
        Div,
        Pow,
    }
    public Operations operation = Operations.Sub;

    /*private static List<string> operators = new List<string>()
    {
        "-",
        "+",
        "*",
        "/",
        "^",
    };*/

    private static List<int> operatorPriorities = new List<int>()
    {
        0,
        0,
        1,
        1,
        1,
    };

    public static List<string> variables = new List<string>
    {
        "x",
        "y",
        "z",
        "pi",
        "e",
        "inf",
    };
    
    public FunctionNode(FunctionNode parent, int level = 0)
    {
        this.parent = parent;
        parenthesisLevel = 0;
    }

    private bool isFloat()
    {
        return !isOperator && !isVariable;
    }

    private void CopyNode(FunctionNode node)
    {
        value = node.value;
        isVariable = node.isVariable;
        isOperator = node.isOperator;
        operation = node.operation;
        variable = node.variable;
        variablePositive = node.variablePositive;
        childLeft = node.childLeft;
        childRight = node.childRight;
    }

    private bool NeedsParenthesis()
    {
        if (parent == null) return false;
        if(parent.childRight == this)
        {
            if (/*parent.operation == Operations.Sub*/operators[parent.operation] == "-")
            {
                if (isOperator || (!isVariable && value < 0f) || (isVariable && !variablePositive)) return true;
            }
        }
        if (!isOperator)
        {
            if (!isVariable && value >= 0f) return false;
            else if (!isVariable && value < 0f) return true;
            if (isVariable && variablePositive) return false;
            else if (isVariable && !variablePositive) return true;
        }
        
        return operatorPriorities[operation] < operatorPriorities[parent.operation];
    }

    private bool NeedsRepresentation()
    {
        if(parent != null && isFloat() && value == 0f)
        {
            //if ((value <= 0f && isFloat()) || (isVariable && !variablePositive)) return true;
            /*if(parent != null)
            {*/
                string op = operators[parent.operation];
                if (op == "+" || op == "-") return false;
            //}
        }
        return true;
    }

    private bool IsLeaf()
    {
        return childLeft == null;
    }

    public void ProcessFunc(string func)
    {
        
        //Remove external parenthesis
        if(func.Length >= 2)
        {
            woreParenthesis = false;
            if ((func.StartsWith("(") || func.StartsWith(")")) && (func.EndsWith("(") || func.EndsWith(")")))
            {
                func = func.Substring(1, func.Length - 2);
                woreParenthesis = true;
                parenthesisLevel++;
            }
        }
        

        //Find possible breakpoints
        int[] coincidences = new int[operators.Count];
        for (int i = 0; i < coincidences.Length; i++) coincidences[i] = -1;
        int funcLength = func.Length;
        int depth = 0;
        for(int i = 0; i < funcLength; i++)
        {
            char c = func[i];
            if (c == '(') depth++;
            if (c == ')') depth--;
            if (depth != 0) continue;
            for(int j = 0; j < coincidences.Length; j++)
            {
                if (coincidences[j] == -1 && (""+c)==operators[j])
                {
                    coincidences[j] = i;
                }
            }
        }

        //Select break point
        bool foundBreakPoint = false;
        int cIdx = 0;
        while(!foundBreakPoint && cIdx < coincidences.Length)
        {
            if(coincidences[cIdx] != -1)
            {
                foundBreakPoint = true;
                int breakPoint = coincidences[cIdx];
                string left = breakPoint == 0 ? "0.0" : func.Substring(0, breakPoint);
                string right = breakPoint == func.Length-1 ? "0.0" : func.Substring(breakPoint + 1);

                /*if((left.StartsWith("(") || left.StartsWith(")")) && (left.EndsWith("(") || left.EndsWith(")")))
                {
                    left = left.Substring(1, left.Length - 2);
                }
                if ((right.StartsWith("(") || right.StartsWith(")")) && (right.EndsWith("(") || right.EndsWith(")")))
                {
                    right = right.Substring(1, right.Length - 2);
                }*/

                childLeft = new FunctionNode(this, parenthesisLevel);
                childRight = new FunctionNode(this, parenthesisLevel);
                childLeft.ProcessFunc(left);
                childRight.ProcessFunc(right);

                isOperator = true;
                operation = cIdx;
                isVariable = false;

                if (operators[operation] == "-")
                {
                    if (childRight.NeedsParenthesis() && !childRight.woreParenthesis)
                    {
                        if (operators[childRight.operation] == "-")
                        {
                            childRight.operation = operators.IndexOf("+");
                        }
                        else if (operators[childRight.operation] == "+")
                        {
                            childRight.operation = operators.IndexOf("-");
                        }
                    }
                }

                TrySimplify();
            }
            cIdx++;
        }

        //If it is not an operation (no breakpoint)
        if (!foundBreakPoint)
        {
            variable = variables.IndexOf(func);
            isVariable = variable >= 0;
            variablePositive = true;
            isOperator = false;
            if (isVariable) value = 0.0f;
            else if (!float.TryParse(func, out value)) value = 0.0f;
        }
    }

    private bool DeepSimplify()
    {
        if (IsLeaf())
        {
            return true;
        } else
        {
            childLeft.DeepSimplify();
            childRight.DeepSimplify();
            return TrySimplify();
        }
    }

    private bool TrySimplify()
    {
        if (childLeft == null) return false;
        bool simplified = false;
        string op = operators[operation];
        if (!childLeft.isOperator && !childLeft.isVariable && !childRight.isOperator && !childRight.isVariable)
        {
            value = Solve();
            isOperator = false;
            isVariable = false;
            operation = 0;
            childLeft = null;
            childRight = null;
            simplified = true;
        } else if(op == "*")
        {
            if ((childLeft.isFloat() && childLeft.value == 0f) || 
                (childRight.isFloat() && childRight.value == 0f)) //Mult por 0
            {
                isOperator = false;
                simplified = true;
                isVariable = false;
                value = 0f;
                childLeft = null;
                childRight = null;
            }
        } else if (op == "/")
        {
            if (childRight.isFloat() && childRight.value == 0f) //denominador 0
            {
                isOperator = false;
                simplified = true;
                isVariable = true;
                variable = variables.IndexOf("inf");
                value = 0f;
                childLeft = null;
                childRight = null;
            }
            else if (childLeft.isFloat() && childLeft.value == 0f) //numerador 0
            {
                isOperator = false;
                simplified = true;
                isVariable = false;
                value = 0f;
                childLeft = null;
                childRight = null;
            }
        } else if (op == "+" || op == "-")
        {
            if (childRight.isFloat() && childRight.value == 0f)
            {
                CopyNode(childLeft);
                if (childLeft != null) childLeft.parent = this;
                if (childRight != null) childRight.parent = this;
                simplified = true;
            }
            else if(childLeft.isFloat() && childLeft.value == 0f && !childRight.isOperator)
            {
                CopyNode(childRight);
                
                if(childLeft != null) childLeft.parent = this;
                if(childRight != null) childRight.parent = this;
                if (op == "-" && !isOperator)
                {
                    variablePositive = false;
                    value = -Mathf.Abs(value);
                }
                simplified = true;
            }
        } else if (op == "^")
        {
            if (childRight.isFloat() && childRight.value == 0f)
            {
                simplified = true;
                isOperator = false;
                isVariable = false;
                value = 1f;
                childLeft = null;
                childRight = null;
            }
            else if (childLeft.isFloat() && childLeft.value == 0f)
            {
                simplified = true;
                isOperator = false;
                isVariable = false;
                value = 0f;
                childLeft = null;
                childRight = null;
            }
        }
        return simplified;
    }

    public float Solve(float x = 0f, float y = 0f, float z = 0f)
    {
        if (isOperator)
        {
            float leftValue = childLeft.Solve(x, y, z);
            float rightValue = childRight.Solve(x, y, z);
            switch (operators[operation])
            {
                case "+":
                    return leftValue + rightValue;
                case "-":
                    return leftValue - rightValue;
                case "*":
                    return leftValue * rightValue;
                case "/":
                    return leftValue / rightValue;
                case "^":
                    return Mathf.Pow(leftValue, rightValue);
                default: goto case "+";
            }
        } else
        {
            float val;
            if (isVariable)
            {
                switch (variables[variable])
                {
                    case "x":
                        val = x;
                        break;
                    case "y":
                        val = y;
                        break;
                    case "z":
                        val = z;
                        break;
                    /*case "pi":
                        val = Mathf.PI;
                        break;
                    case "e":
                        val = Mathf.Exp(1f);
                        break;
                    case "inf":
                        val = Mathf.Infinity * 0.8f;
                        break;*/
                    default:
                        FunctionManager.TryGetVariable(variables[variable], out val);
                        break;
                }
                val = variablePositive ? val : -val;
            }
            else val = value;
            return val;
        }
    }

    public override string ToString()
    {
        if (!NeedsRepresentation()) return "";
        string aux = "";
        bool needsParenthesis = NeedsParenthesis();
        if(needsParenthesis) aux += '(';
        if (isOperator) aux += childLeft.ToString() + operators[operation] + childRight.ToString();
        else if (isVariable) aux+= (variablePositive ? "":"-")+ variables[variable];
        else aux += "" + value;
        if (needsParenthesis) aux += ")";
        return aux;
    }

}
