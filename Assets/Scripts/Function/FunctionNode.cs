using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System;

public class FunctionNode
{
    private FunctionNode parent;
    private FunctionNode childLeft;
    private FunctionNode childRight;
    private bool isOperator;
    private string operation = "+";
    private float value;
    private bool isVariable;
    private string variable;
    private bool variablePositive = true;
    private bool isSubFunction = false;
    private string subFunction = "";

    private bool woreParenthesis = false;
    private int parenthesisLevel = 0;

    #region static

    private static List<string> operators = new List<string>()
    {
        "-",
        "+",
        "*",
        "/",
        "^",
    };

    private static Dictionary<string, int> operatorPriorities = new Dictionary<string, int>()
    {
        { "-", 0 },
        { "+", 0 },
        { "*", 1 },
        { "/", 1 },
        { "^", 1 },
    };

    public static List<string> variables = new List<string>
    {
        "x",
        "y",
        "z",
    };

    public static List<string> subFunctions = new List<string>
    {
        "cos",
        "sin",
        "abs",
    };

    #endregion

    public FunctionNode(FunctionNode parent, int level = 0)
    {
        this.parent = parent;
        parenthesisLevel = 0;
    }

    private void CopyNode(FunctionNode node)
    {
        value = node.value;
        isVariable = node.isVariable;
        isOperator = node.isOperator;
        isSubFunction = node.isSubFunction;
        subFunction = node.subFunction;
        operation = node.operation;
        variable = node.variable;
        variablePositive = node.variablePositive;
        childLeft = node.childLeft;
        childRight = node.childRight;
    }

    #region complexProperties

    private bool isFloat()
    {
        return !isOperator && !isVariable && !isSubFunction;
    }

    private bool NeedsParenthesis()
    {
        if (parent == null) return false;
        if(parent.childRight == this)
        {
            if (parent.operation == "-")
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
            string op = parent.operation;
            if (op == "+" || op == "-") return false;
        }
        return true;
    }

    private bool IsLeaf()
    {
        return childLeft == null && childRight == null && !isSubFunction;
    }

    #endregion

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
                childLeft = new FunctionNode(this, parenthesisLevel);
                childRight = new FunctionNode(this, parenthesisLevel);
                childLeft.ProcessFunc(left);
                childRight.ProcessFunc(right);

                isOperator = true;
                operation = operators[cIdx];
                isVariable = false;

                if (operation == "-")
                {
                    if (childRight.NeedsParenthesis() && !childRight.woreParenthesis)
                    {
                        if (childRight.operation == "-") childRight.operation = "+";
                        else if (childRight.operation == "+") childRight.operation = "-";
                    }
                }

                TrySimplify();
            }
            cIdx++;
        }

        //If it is not an operation (no breakpoint)
        if (!foundBreakPoint)
        {
            isVariable = false;
            isOperator = false;
            variablePositive = true;
            isSubFunction = false;
            value = 0f;
            foreach (var n in FunctionManager.functions.Keys)
            {
                if (func.StartsWith(n))
                {
                    isSubFunction = true;
                    subFunction = n;
                    break;
                }
            }
            if (!isSubFunction)
            {
                foreach (var n in subFunctions)
                {
                    if (func.StartsWith(n))
                    {
                        isSubFunction = true;
                        subFunction = n;
                        break;
                    }
                }
            }
            if (isSubFunction)
            {
                string content = "";
                bool insideContent = false;
                for(int i = 0; i < func.Length; i++)
                {
                    if (!insideContent)
                    {
                        insideContent = func[i] == '(';
                    } else
                    {
                        if (func[i] == ')') insideContent = false;
                        else
                        {
                            content += func[i];
                        }
                    }
                    
                }
                childLeft = null;
                childRight = new FunctionNode(this, parenthesisLevel + 1);
                childRight.ProcessFunc(content);
            }

            else
            {
                if (variables.Contains(func) || FunctionManager.HasVariable(func))
                {
                    variable = func;
                    isVariable = true;
                }

                if (isVariable) value = 0.0f;

                try
                {
                    value = float.Parse(func.Replace(",", "."), CultureInfo.InvariantCulture);
                } catch(Exception e)
                {
                    value = 0f;
                }
            }
        }
    }

    #region Simplify

    private bool DeepSimplify()
    {
        if (IsLeaf())
        {
            return true;
        } else
        {
            childLeft?.DeepSimplify();
            childRight?.DeepSimplify();
            return TrySimplify();
        }
    }

    private bool TrySimplify()
    {
        if (IsLeaf()) return false;
        if (isSubFunction || (parent != null && parent.isSubFunction)) return false;
        bool simplified = false;
        string op = operation;
        if (/*!childLeft.isOperator && !childLeft.isVariable && !childRight.isOperator && !childRight.isVariable*/childLeft.isFloat() && childRight.isFloat())
        {
            value = Solve();
            isOperator = false;
            isVariable = false;
            operation = "+";
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
            } else if((childLeft.isFloat() && childLeft.value == 1f) ||
                (childRight.isFloat() && childRight.value == 1f))
            {
                isOperator = false;
                simplified = true;
                isVariable = false;
                //childLeft = null;
                //childRight = null;
                if (childLeft.isFloat() && childLeft.value == 1f) CopyNode(childRight);
                else CopyNode(childLeft);
            }
        } else if (op == "/")
        {
            if (childRight.isFloat() && childRight.value == 0f) //denominador 0
            {
                isOperator = false;
                simplified = true;
                isVariable = true;
                variable = "inf";
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
            } else if (childRight.isFloat() && childRight.value == 1f)
            {
                isOperator = false;
                simplified = true;
                isVariable = false;
                //childLeft = null;
                //childRight = null;
                CopyNode(childLeft);
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

    #endregion

    public float Solve(float x = 0f, float y = 0f, float z = 0f)
    {
        if (isSubFunction)
        {
            float rightValue = 0f;
            if(childRight != null)
            {
                rightValue = childRight.Solve(x, y, z);
            }
            switch (subFunction)
            {
                case "sin": return Mathf.Sin(rightValue);
                case "cos": return Mathf.Cos(rightValue);
                case "abs": return Mathf.Abs(rightValue);
                default:
                    Function subF;
                    if(FunctionManager.functions.TryGetValue(subFunction, out subF)){
                        rightValue = subF.rootNode.Solve(x, y, z);
                    }
                    return rightValue;
            }
        }
        if (isOperator)
        {
            float leftValue = childLeft.Solve(x, y, z);
            float rightValue = childRight.Solve(x, y, z);
            switch (operation)
            {
                case "+": return leftValue + rightValue;
                case "-": return leftValue - rightValue;
                case "*": return leftValue * rightValue;
                case "/": return leftValue / rightValue;
                case "^": return Mathf.Pow(leftValue, rightValue);
                default: goto case "+";
            }
        } else
        {
            float val;
            if (isVariable)
            {
                switch (variable)
                {
                    case "x": val = x; break;
                    case "y": val = y; break;
                    case "z": val = z; break;
                    default: FunctionManager.TryGetVariable(variable, out val); break;
                }
                val = variablePositive ? val : -val;
            }
            else val = value;
            return val;
        }
    }

    public override string ToString()
    {
        if (isSubFunction) return subFunction + "(" + childRight.ToString() + ")";
        if (!NeedsRepresentation()) return "";
        string aux = "";
        bool needsParenthesis = NeedsParenthesis();
        if(needsParenthesis) aux += '(';
        if (isOperator) aux += childLeft.ToString() + operation + childRight.ToString();
        else if (isVariable) aux+= (variablePositive ? "":"-")+ variable;
        else aux += "" + value;
        if (needsParenthesis) aux += ")";
        return aux;
    }
}
