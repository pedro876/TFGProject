using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FuncSpace
{
    public class FuncReader
    {
        private FuncFactory factory;

        public FuncReader()
        {
            factory = FuncFactory.Instance;
        }

        public IFunc ExtractOriginalFuncInfo(string textFunc, IFunc func)
        {
            string cleanInput = CleanInput(textFunc);
            ExtractOriginalDeclarationAndDefinition(cleanInput, func);
            ExtractNameFromDeclaration(func);
            FixOriginalDefinition(func);
            return func;
        }

        private string CleanInput(string input)
        {
            input = input.Replace(" ", "");
            input = input.ToLower();
            return input;
        }

        private void ExtractOriginalDeclarationAndDefinition(string input, IFunc func)
        {
            string[] parts = input.Split('=');
            string originalDeclaration = parts[0];
            string originalDefinition = parts[1];
            func.SetOriginalDeclaration(originalDeclaration);
            func.SetOriginalDefinition(originalDefinition);
        }

        private void ExtractNameFromDeclaration(IFunc func)
        {
            string name = func.GetOriginalDeclaration().Split('(')[0];
            func.SetName(name);
        }

        private void FixOriginalDefinition(IFunc func)
        {
            string definition = func.GetOriginalDefinition();
            if (definition.Length == 0)
                definition = "0";
            else
            {
                definition = CheckIfMulOmittedForParenthesis(definition);
                definition = CheckIfMulOmittedForVariables(definition);
            }

            func.SetOriginalDefinition(definition);
        }

        private string CheckIfMulOmittedForParenthesis(string definition)
        {
            string aux = "";
            string symbols = "+-*^/()";
            string cut = definition;
            for (int i = 0; i < definition.Length - 1; i++)
            {
                bool startsWithSubFunc = StartsWithSubFunction(cut, out string subFunction);

                if (startsWithSubFunc)
                {
                    i += subFunction.Length;
                    aux += subFunction + "(";
                    cut = cut.Substring(subFunction.Length + (cut.Length > subFunction.Length ? 1 : 0));
                }
                else
                {
                    bool mustAddMulSymbolLeft = i > 0 && definition[i] == '(' && !symbols.Contains("" + definition[i - 1]);
                    bool mustAddMulSymbolRight = definition[i] == ')' && !symbols.Contains("" + definition[i + 1]);
                    
                    if (mustAddMulSymbolLeft) aux += '*';
                    aux += definition[i];
                    if (mustAddMulSymbolRight) aux += "*";

                    cut = cut.Substring(1);
                }
            }

            aux += definition[definition.Length - 1];
            definition = aux;
            return definition;
        }

        private bool StartsWithSubFunction(string text, out string subFunction)
        {
            bool startsWithSubFunc = false;
            subFunction = "";
            foreach (var n in factory.AllFuncNames)
            {
                if (text.StartsWith(n))
                {
                    startsWithSubFunc = true;
                    subFunction = n;
                    break;
                }
            }
            return startsWithSubFunc;
        }

        private string CheckIfMulOmittedForVariables(string definition)
        {
            const string numberSymbols = "0123456789.";

            string aux = "";
            string cutDef = definition;
            bool previousVar = false;
            bool previousNumber = false;
            while (cutDef.Length > 0)
            {
                if(StartsWithVariable(cutDef, out string variable))
                {
                    if (previousVar || previousNumber)
                    {
                        aux += '*';
                    }
                    previousVar = true;
                    previousNumber = false;
                    aux += variable;
                    cutDef = cutDef.Substring(variable.Length);
                    //if (cutDef.Length > 0 && numberSymbols.Contains("" + cutDef[0])) aux += '*';
                } else
                {
                    previousVar = false;
                    previousNumber = numberSymbols.Contains("" + cutDef[0]);
                    aux += cutDef[0];
                    cutDef = cutDef.Substring(1);
                }
            }
            definition = aux;
            return definition;
        }

        private bool StartsWithVariable(string text, out string variable)
        {
            var variables = factory.Variables;
            variable = "";
            foreach (string v in variables)
            {
                if (text.StartsWith(v))
                {
                    variable = v;
                    return true;
                }
            }
            return false;
        }

        public IFunc ExtractFinalFuncInfo(IFunc func)
        {
            ExtractFinalDefinition(func);
            ExtractVariables(func);
            ExtractSubfunctions(func);
            ExtractFinalDeclaration(func);
            return func;
        }

        private void ExtractFinalDefinition(IFunc func)
        {
            string finalDefinition = func.ComputeDefinitionString();
            func.SetFinalDefinition(finalDefinition);
        }

        private void ExtractVariables(IFunc func)
        {
            List<string> variables = new List<string>();
            string finalDefinition = func.GetFinalDefinition();
            foreach (var variable in factory.Variables)
            {
                if (finalDefinition.Contains(variable))
                    variables.Add(variable);
            }
            func.SetVariables(variables);
        }

        private void ExtractSubfunctions(IFunc func)
        {
            List<string> subfunctions = new List<string>();
            string finalDefinition = func.GetFinalDefinition();
            foreach (var subfunction in factory.AllFuncNames)
            {
                if (factory.IsFuncUserDefined(subfunction))
                {
                    if (finalDefinition.Contains(subfunction))
                    {
                        subfunctions.Add(subfunction);
                    }
                }
            }
            func.SetSubfunctions(subfunctions);
        }

        private void ExtractFinalDeclaration(IFunc func)
        {
            StringBuilder finalDeclaration = new StringBuilder(func.GetName());
            int variableCount = func.GetVariables().Count;
            if (variableCount > 0)
            {
                finalDeclaration.Append("(");
                for (int i = 0; i < variableCount; i++)
                {
                    finalDeclaration.Append(func.GetVariables()[i]);
                    if (i > 0) finalDeclaration.Append(",");
                }
                finalDeclaration.Append(")");
            }
            func.SetFinalDeclaration(finalDeclaration.ToString());
        }
    }
}