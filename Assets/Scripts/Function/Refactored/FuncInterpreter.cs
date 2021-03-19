using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuncSpace
{
    public class FuncInterpreter
    {
        FuncFactory factory;

        public FuncInterpreter()
        {
            factory = FuncFactory.Instance;
        }

        public void CreateNodeTreeForFunc(IFunc func)
        {
            string definition = func.GetOriginalDefinition();
            IFuncNode root = ProcessDefinition(definition);
            func.SetRootNode(root);
        }

        private IFuncNode ProcessDefinition(string definition)
        {
            IFuncNode node;
            bool insideParenthesis = CheckExternalParenthesis(ref definition);
            int[] breakpoints = FindPossibleBreakpoints(ref definition);
            int breakpoint = SelectBreakPoint(breakpoints);

            bool isOperation = breakpoint > 0;
            if (isOperation)
            {
                node = ProcessOperation(ref definition, breakpoint, breakpoints[breakpoint]);
            } else
            {
                if(IsSubfunction(ref definition, out string subfunction))
                {
                    node = ProcessSubfunction(ref definition, ref subfunction);
                }
                else if (IsVariable(ref definition))
                {
                    node = ProcessVariable(ref definition);
                } else
                {
                    node = ProcessConstant(ref definition);
                }
            }
            return node;
        }

        private bool CheckExternalParenthesis(ref string definition)
        {
            if (definition.Length >= 2)
            {
                if (definition.StartsWith("(") && definition.EndsWith(")"))
                {
                    definition = definition.Substring(1, definition.Length - 2);
                    return true;
                }
            }
            return false;
        }

        #region breakpoint

        private int[] FindPossibleBreakpoints(ref string definition)
        {
            var operators = factory.Operators;
            int[] breakPoints = new int[operators.Count];
            for (int i = 0; i < breakPoints.Length; i++) breakPoints[i] = -1;
            int funcLength = definition.Length;
            int depth = 0;
            for (int i = 0; i < funcLength; i++)
            {
                char c = definition[i];
                if (c == '(') depth++;
                if (c == ')') depth--;
                if (depth != 0) continue;
                for (int j = 0; j < breakPoints.Length; j++)
                {
                    if (breakPoints[j] == -1 && ("" + c) == operators[j])
                    {
                        breakPoints[j] = i;
                    }
                }
            }
            return breakPoints;
        }

        private int SelectBreakPoint(int[] breakpoints)
        {
            for(int i = 0; i < breakpoints.Length; i++)
            {
                if (breakpoints[i] != -1) return i;
            }
            return -1;
        }

        #endregion

        #region Operator

        private IFuncNode ProcessOperation(ref string definition, int breakpoint, int splitPoint)
        {
            string operation = factory.Operators[breakpoint];
            var children = ProcessOperatorChildren(ref definition, splitPoint);
            IFuncNode node = CreateOperatorNodeFromString(operation, children);
            foreach (var child in children)
            {
                child.SetParent(node);
            }
            if (node is OperatorSubNode)
            {
                CheckRightChildNegation(children);
            }
            return node;
        }

        private List<IFuncNode> ProcessOperatorChildren(ref string definition, int splitPoint)
        {
            string left = splitPoint == 0 ? "0.0" : definition.Substring(0, splitPoint);
            string right = splitPoint == definition.Length - 1 ? "0.0" : definition.Substring(splitPoint + 1);

            return new List<IFuncNode>()
            {
                ProcessDefinition(left),
                ProcessDefinition(right),
            };
        }

        private IFuncNode CreateOperatorNodeFromString(string operation, List<IFuncNode> children)
        {

            switch (operation)
            {
                case "-": return new OperatorSubNode(children);
                case "+": return new OperatorAddNode(children);
                case "*": return new OperatorMulNode(children);
                case "/": return new OperatorDivNode(children);
                case "^": return new OperatorPowNode(children);
                default: goto case "+";
            }
        }

        private void CheckRightChildNegation(List<IFuncNode> children)
        {
            OperatorNode opNode = (OperatorNode)children[1];
            if (opNode.NeedsParenthesis() && !opNode.IsInsideParenthesis())
            {
                if (opNode is OperatorSubNode)
                {
                    opNode = new OperatorAddNode(opNode.GetChildren());
                }
                else if (opNode is OperatorAddNode)
                {
                    opNode = new OperatorSubNode(opNode.GetChildren());
                }
            }
            children[1] = opNode;
        }

        #endregion

        #region Subfunction

        private bool IsSubfunction(ref string definition, out string subfunction)
        {
            foreach(var name in factory.AllFuncNames)
            {
                if (definition.StartsWith(name))
                {
                    subfunction = name;
                    return true;
                }
            }
            subfunction = "";
            return false;
        }

        private IFuncNode ProcessSubfunction(ref string definition, ref string subfunction)
        {
            string content = GetSubfunctionContent(ref definition, ref subfunction);
            var children = ProcessSubfunctionChildren(content);
           
            IFuncNode node;
            if (factory.IsFuncUserDefined(subfunction))
            {
                node = new UserDefinedFuncNode(subfunction, children);
            } else
            {
                node = new PredefinedFuncNode(subfunction, children);
            }

            foreach (var child in children)
            {
                child.SetParent(node);
            }
            return node;
        }

        private string GetSubfunctionContent(ref string definition, ref string subfunction)
        {
            string content = definition.Substring(subfunction.Length);
            if (content.Length > 2 && content[0] == '(' && content[content.Length - 1] == ')')
            {
                content = content.Substring(1, content.Length - 2);
            }
            else content = "0";
            return content;
        }

        private List<IFuncNode> ProcessSubfunctionChildren(string content)
        {
            string[] inputs = content.Split(',');
            List<IFuncNode> children = new List<IFuncNode>();
            foreach (var input in inputs)
            {
                children.Add(ProcessDefinition(input));
            }
            return children;
        }

        #endregion

        #region Variable

        private bool IsVariable(ref string definition)
        {
            return factory.Variables.Contains(definition);
        }

        private IFuncNode ProcessVariable(ref string definition)
        {
            return new VariableNode(definition);
        }

        #endregion

        #region Constant

        private IFuncNode ProcessConstant(ref string definition)
        {
            return new ConstantNode(definition);
        }

        #endregion
    }
}