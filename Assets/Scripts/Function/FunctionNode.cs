using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System;
using System.Text;

public class FunctionNode
{
    private FunctionNode parent;
    private FunctionNode childLeft;
    private FunctionNode childRight;
    private bool isOperator;
    private string operation = "+";
    private int operationIndex = 0;
    private bool isVariable;
    private string variable;
    private bool variablePositive = true;
    private bool isSubFunction = false;
    private string subFunction = "";
    private int subFunctionIndex = -1;
    private FunctionNode[] subFunctionInputs = null;
    private bool woreParenthesis = false;
    //private int parenthesisLevel = 0;

    #region static

    private static List<string> operators = new List<string>()
    {
        "-",
        "+",
        "*",
        "/",
        "^",
    };

    public static int GetMaxOperatorIndex { get => operators.Count; }

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

    #region classCreation

    public FunctionNode(FunctionNode parent/*, int level = 0*/)
    {
        this.parent = parent;
        //parenthesisLevel = 0;
    }

    private void CopyNode(FunctionNode node)
    {
        Value = node.Value;
        isVariable = node.isVariable;
        isOperator = node.isOperator;
        isSubFunction = node.isSubFunction;
        subFunction = node.subFunction;
        subFunctionIndex = node.subFunctionIndex;
        operation = node.operation;
        operationIndex = node.operationIndex;
        variable = node.variable;
        variablePositive = node.variablePositive;
        childLeft = node.childLeft;
        childRight = node.childRight;
        subFunctionInputs = node.subFunctionInputs;
    }

    #endregion

    #region complexProperties

    public float Value { get; private set; }
    public bool IsFloat { get => IsLeaf && !isVariable; }
    private bool IsLeaf { get => childLeft == null && childRight == null && !isSubFunction; }

    private bool NeedsParenthesis()
    {
        if (parent == null) return false;
        if (parent.childRight == this && parent.operation == "-" &&
            (isOperator || (!isVariable && Value < 0f) || (isVariable && !variablePositive)))
        {
            return true;
        }
        if (!isOperator)
        {
            if (!isVariable && Value >= 0f) return false;
            else if (!isVariable && Value < 0f) return true;
            if (isVariable && variablePositive) return false;
            else if (isVariable && !variablePositive) return true;
        }
        
        return operatorPriorities[operation] < operatorPriorities[parent.operation];
    }

    private bool NeedsRepresentation()
    {
        if(parent != null && IsFloat && Value == 0f)
        {
            string op = parent.operation;
            if (op == "+" || op == "-") return false;
        }
        return true;
    }

    #endregion

    #region functionProcessing

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
                //parenthesisLevel++;
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
                childLeft = new FunctionNode(this/*, parenthesisLevel*/);
                childRight = new FunctionNode(this/*, parenthesisLevel*/);
                childLeft.ProcessFunc(left);
                childRight.ProcessFunc(right);

                isOperator = true;
                operation = operators[cIdx];
                operationIndex = cIdx;
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
            Value = 0f;
            foreach (var n in FunctionManager.functions.Keys)
            {
                if (func.StartsWith(n))
                {
                    isSubFunction = true;
                    subFunction = n;
                    subFunctionIndex = -1;
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
                        subFunctionIndex = subFunctions.IndexOf(subFunction);
                        break;
                    }
                }
            }
            if (isSubFunction)
            {
                string content = func.Substring(subFunction.Length);
                if (content.Length > 2 && content[0] == '(' && content[content.Length - 1] == ')')
                {
                    content = content.Substring(1, content.Length - 2);
                }
                else content = "0";

                string[] inputs = content.Split(',');

                childLeft = null;
                childRight = null;
                int maxInput = inputs.Length > 3 ? 3 : inputs.Length;
                subFunctionInputs = new FunctionNode[maxInput];
                for(int i = 0; i < maxInput; i++)
                {
                    subFunctionInputs[i] = new FunctionNode(this/*, parenthesisLevel + 1*/);
                    subFunctionInputs[i].ProcessFunc(inputs[i]);
                }
            }

            else
            {
                if (variables.Contains(func) || FunctionManager.HasVariable(func))
                {
                    variable = func;
                    isVariable = true;
                }

                if (isVariable) Value = 0.0f;

                try
                {
                    Value = float.Parse(func.Replace(",", "."), CultureInfo.InvariantCulture);
                } catch (Exception)
                {
                    Value = 0f;
                }
            }
        }
    }

    #endregion

    #region Simplify

    private bool DeepSimplify()
    {
        if (IsLeaf)
        {
            return true;
        } else
        {
            childLeft?.DeepSimplify();
            childRight?.DeepSimplify();
            if (subFunctionInputs != null)
            {
                foreach (FunctionNode fn in subFunctionInputs) fn.DeepSimplify();
            }
            return TrySimplify();
        }
    }

    private bool TrySimplify()
    {
        if (IsLeaf) return false;
        if (isSubFunction || (parent != null && parent.isSubFunction)) return false;
        bool simplified = false;
        string op = operation;
        if (childLeft.IsFloat && childRight.IsFloat)
        {
            Value = Solve();
            isOperator = false;
            isVariable = false;
            operation = "+";
            childLeft = null;
            childRight = null;
            simplified = true;
        } else if(op == "*")
        {
            if ((childLeft.IsFloat && childLeft.Value == 0f) || 
                (childRight.IsFloat && childRight.Value == 0f)) //Mult por 0
            {
                isOperator = false;
                simplified = true;
                isVariable = false;
                Value = 0f;
                childLeft = null;
                childRight = null;
            } else if((childLeft.IsFloat && childLeft.Value == 1f) ||
                (childRight.IsFloat && childRight.Value == 1f))
            {
                isOperator = false;
                simplified = true;
                isVariable = false;
                //childLeft = null;
                //childRight = null;
                if (childLeft.IsFloat && childLeft.Value == 1f) CopyNode(childRight);
                else CopyNode(childLeft);
            }
        } else if (op == "/")
        {
            if (childRight.IsFloat && childRight.Value == 0f) //denominador 0
            {
                isOperator = false;
                simplified = true;
                isVariable = true;
                variable = "inf";
                Value = 0f;
                childLeft = null;
                childRight = null;
            }
            else if (childLeft.IsFloat && childLeft.Value == 0f) //numerador 0
            {
                isOperator = false;
                simplified = true;
                isVariable = false;
                Value = 0f;
                childLeft = null;
                childRight = null;
            } else if (childRight.IsFloat && childRight.Value == 1f)
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
            if (childRight.IsFloat && childRight.Value == 0f)
            {
                CopyNode(childLeft);
                if (childLeft != null) childLeft.parent = this;
                if (childRight != null) childRight.parent = this;
                simplified = true;
            }
            else if(childLeft.IsFloat && childLeft.Value == 0f && !childRight.isOperator)
            {
                CopyNode(childRight);
                
                if(childLeft != null) childLeft.parent = this;
                if(childRight != null) childRight.parent = this;
                if (op == "-" && !isOperator)
                {
                    variablePositive = false;
                    Value = -Mathf.Abs(Value);
                }
                simplified = true;
            }
        } else if (op == "^")
        {
            if (childRight.IsFloat && childRight.Value == 0f)
            {
                simplified = true;
                isOperator = false;
                isVariable = false;
                Value = 1f;
                childLeft = null;
                childRight = null;
            }
            else if (childLeft.IsFloat && childLeft.Value == 0f)
            {
                simplified = true;
                isOperator = false;
                isVariable = false;
                Value = 0f;
                childLeft = null;
                childRight = null;
            }
        }
        return simplified;
    }

    #endregion

    #region Solve

    public float Solve(float x = 0f, float y = 0f, float z = 0f)
    {
        if (isSubFunction)
        {
            float[] values = new float[3];
            for (int i = 0; i < subFunctionInputs.Length; i++)
            {
                values[i] = subFunctionInputs[i].Solve(x, y, z);
            }

            if (subFunctionIndex >= 0)
            {
                return SolveSubFunction(subFunctionIndex, values[0], values[1], values[2]);
            } else
            {
                Function subF;
                if (FunctionManager.functions.TryGetValue(subFunction, out subF))
                {
                    return subF.Solve(values[0], values[1], values[2]);
                }
                else return 0f;
            }
        } 
        if (isOperator)
        {
            float leftValue = childLeft.Solve(x, y, z);
            float rightValue = childRight.Solve(x, y, z);
            return SolveOperator(operationIndex, leftValue, rightValue);
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
            else val = Value;
            return val;
        }
    }

    public static float SolveSubFunction(int op, float v0, float v1, float v2)
    {
        if (op == 0) return Mathf.Cos(v0);
        else if (op == 1) return Mathf.Sin(v0);
        else if (op == 2) return Mathf.Abs(v0);
        return 0f;
    }

    public static float SolveOperator(int op, float v0, float v1)
    {
        if (op == 0) return v0 - v1;
        else if (op == 1) return v0 + v1;
        else if (op == 2) return v0 * v1;
        else if (op == 3) return v0 / v1;
        else if (op == 4) return Mathf.Pow(v0, v1);
        return 0f;
    }

    #endregion

    #region bytecode

    public int CalculateBytecode(Dictionary<FunctionNode, int> memoryNodes, List<int> bytecode, HashSet<string> subFuncs, int lastMemoryIndex = 4, int max = 256, int[] varIdx = null)
    {
        if (varIdx == null) varIdx = new int[] { 1, 2, 3 };
        int memoryIndex;
        if(lastMemoryIndex == max)
        {
            return lastMemoryIndex;
        }
        else if (IsLeaf)
        {
            if (isVariable)
            {
                switch (variable)
                {
                    case "x": memoryIndex = varIdx[0]; break;
                    case "y": memoryIndex = varIdx[1]; break;
                    case "z": memoryIndex = varIdx[2]; break;
                    default: memoryIndex = 0; break;
                }
                if (!variablePositive) memoryIndex = -memoryIndex;
            } else
            {
                memoryIndex = lastMemoryIndex;
                lastMemoryIndex++; ;
            }
        } else
        {
            if (isSubFunction)
            {
                int[] subFuncVarIdx = new int[3];
                for(int i = 0; i < subFunctionInputs.Length; i++)
                {
                    lastMemoryIndex = subFunctionInputs[i].CalculateBytecode(memoryNodes, bytecode, subFuncs, lastMemoryIndex, max, varIdx);
                    subFuncVarIdx[i] = memoryNodes[subFunctionInputs[i]];
                }
                if (!subFunctions.Contains(subFunction))
                {
                    Function subF;
                    if (FunctionManager.functions.TryGetValue(subFunction, out subF))
                    {
                        if (!subFuncs.Contains(subF.name))
                        {
                            subFuncs.Add(subF.name);
                            lastMemoryIndex = subF.rootNode.CalculateBytecode(memoryNodes, bytecode, subFuncs, lastMemoryIndex, max, subFuncVarIdx);
                            subFuncs.Remove(subF.name);
                            memoryIndex = memoryNodes[subF.rootNode];
                        }
                        else memoryIndex = 0;
                    } else memoryIndex = 0;
                }
                else
                {
                    bytecode.Add(operators.Count + subFunctions.IndexOf(subFunction));
                    bytecode.Add(subFuncVarIdx[0]); //x value index
                    bytecode.Add(subFuncVarIdx[1]); //y value index
                    bytecode.Add(subFuncVarIdx[2]); //z value index
                    memoryIndex = lastMemoryIndex;
                    lastMemoryIndex += 1;
                    bytecode.Add(memoryIndex); //Where the result will be stored
                }
            }
            else
            {
                lastMemoryIndex = childLeft.CalculateBytecode(memoryNodes, bytecode, subFuncs, lastMemoryIndex, max, varIdx);
                lastMemoryIndex = childRight.CalculateBytecode(memoryNodes, bytecode, subFuncs, lastMemoryIndex, max, varIdx);

                bytecode.Add(operators.IndexOf(operation)); //operation index
                bytecode.Add(memoryNodes[childLeft]); //Left value
                bytecode.Add(memoryNodes[childRight]); //Right value

                memoryIndex = /*memoryNodes[childLeft]*/lastMemoryIndex;
                lastMemoryIndex++;
                bytecode.Add(memoryIndex); //Where the result will be stored
            }
            
        }
        memoryNodes[this] = memoryIndex;
        return lastMemoryIndex;
    }

    #endregion

    #region ToString

    private void ToStringDeep(StringBuilder builder)
    {
        if (!NeedsRepresentation()) return;
        if (isSubFunction)
        {
            builder.Append(subFunction);
            builder.Append('(');
            for (int i = 0; i < subFunctionInputs.Length; i++)
            {
                subFunctionInputs[i].ToStringDeep(builder);
                if (i < subFunctionInputs.Length - 1) builder.Append(',');
            }
            builder.Append(')');
            return;
        }

        bool needsParenthesis = NeedsParenthesis();
        if (needsParenthesis) builder.Append('(');
        if (isOperator)
        {
            childLeft.ToStringDeep(builder);
            builder.Append(operation);
            childRight.ToStringDeep(builder);
        }
        else if (isVariable)
        {
            if (!variablePositive) builder.Append('-');
            builder.Append(variable);
        }
        else builder.Append(Value.ToString());
        if (needsParenthesis) builder.Append(')');
        return;
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        ToStringDeep(builder);
        return builder.ToString();
    }

    #endregion
}