using System.Collections.Generic;
using System.Text;

namespace FuncSpace
{
    public class FuncReader : IFuncReader
    {

        IFuncFactory factory;

        public FuncReader(IFuncFactory factory)
        {
            this.factory = factory;
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
            //input = input.ToLower();
            return input;
        }

        private void ExtractOriginalDeclarationAndDefinition(string input, IFunc func)
        {
            string[] parts = input.Split('=');
            string originalDeclaration = parts[0];
            string originalDefinition = parts.Length > 1 ? parts[1] : "0";
            originalDefinition = originalDefinition.ToLower();
            func.OriginalDeclaration = originalDeclaration;
            func.OriginalDefinition = originalDefinition;
        }

        private void ExtractNameFromDeclaration(IFunc func)
        {
            string name = func.OriginalDeclaration.Split('(')[0];
            if(name.Length == 0)
            {
                name = "undefined";
            }
            func.Name = name;
        }

        private void FixOriginalDefinition(IFunc func)
        {
            string definition = func.OriginalDefinition;
            if (definition.Length == 0)
                definition = "0";
            else
            {
                definition = CheckIfMulOmittedForParenthesis(definition);
                definition = CheckIfMulOmittedForVariables(definition);
            }

            func.OriginalDefinition = definition;
        }

        private string CheckIfMulOmittedForParenthesis(string definition)
        {
            string aux = "";
            
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
                    bool mustAddMulSymbolLeft = i > 0 && definition[i] == '(' && IsNumberOrVariable(definition[i - 1].ToString());
                    bool mustAddMulSymbolRight = definition[i] == ')' && IsNumberOrVariable(definition[i + 1].ToString());
                    
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

        private bool IsNumberOrVariable(string c)
        {
            const string symbols = "+-*^/()";
            const string numberSymbols = "0123456789.";

            bool isOperator = symbols.Contains(c);
            bool isVariable = FuncGeneralInfo.VariableExists(c);
            bool isNumber = numberSymbols.Contains(c);

            return (isVariable || isNumber) && !isOperator;
        }

        private bool StartsWithSubFunction(string text, out string subFunction)
        {
            bool startsWithSubFunc = false;
            string auxSub = "";
            int i = 0;
            StringBuilder b = new StringBuilder();
            while(i < text.Length && text[i] != '(')
            {
                b.Append(text[i]);
                i++;
            }
            string subF = b.ToString();
            factory.ForEachFuncName((name) =>
            {
                if (subF.Equals(name))
                {
                    startsWithSubFunc = true;
                    auxSub = name;
                }
            });
            subFunction = auxSub;
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
            //var variables = FuncGeneralInfo.variables;
            string aux = "";
            bool foundCoincidence = false;
            FuncGeneralInfo.ForEachVariable((variable) =>
            {
                if (text.StartsWith(variable))
                {
                    aux = variable;
                    foundCoincidence = true;
                }
            });
            variable = aux;
            return foundCoincidence;
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
            func.FinalDefinition = finalDefinition;
        }

        private void ExtractVariables(IFunc func)
        {
            List<string> variables = new List<string>();
            string finalDefinition = func.FinalDefinition;
            FuncGeneralInfo.ForEachVariable((variable) =>
            {
                if (finalDefinition.Contains(variable))
                    variables.Add(variable);
            });
            func.Variables = variables;
        }

        private void ExtractSubfunctions(IFunc func)
        {
            List<string> subfunctions = new List<string>();
            string finalDefinition = func.FinalDefinition;
            factory.ForEachFuncName((name) =>
            {
                if (factory.IsFuncDefinedByUser(name))
                {
                    if (finalDefinition.Contains(name))
                    {
                        subfunctions.Add(name);
                    }
                }
            });
            func.Subfunctions = subfunctions;
        }

        private void ExtractFinalDeclaration(IFunc func)
        {
            StringBuilder finalDeclaration = new StringBuilder(func.Name);
            int variableCount = func.Variables.Count;
            if (variableCount > 0)
            {
                finalDeclaration.Append("(");
                for (int i = 0; i < variableCount; i++)
                {
                    if (i > 0) finalDeclaration.Append(",");
                    finalDeclaration.Append(func.Variables[i]);
                }
                finalDeclaration.Append(")");
            }
            func.FinalDeclaration = finalDeclaration.ToString();
        }
    }
}