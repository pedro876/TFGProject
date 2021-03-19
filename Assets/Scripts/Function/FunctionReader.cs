using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class FunctionReader
{
    /*
     * Creates a function based on a human-comprehensible mathematical function
     */
    public FunctionC ProcessFunc(string funcText)
    {
        funcText = funcText.ToLower();
        string trimmedFunc = funcText.Replace(" ", "");
        string[] parts = trimmedFunc.Split('=');
        if (parts.Length < 2) return null;
        string name;
        string declaration = ProcessDeclaration(parts[0], ref parts[1], out name);
        string definition = ProcessDefinition(parts[1]);

        FunctionC func = new FunctionC(name, declaration, parts[1], definition);
        
        return func;
    }

    /*
     * Check if some coordinate variable is missing in the declaration and fix it
     */
    private string ProcessDeclaration(string declaration, ref string definition, out string name)
    {
        StringBuilder funcName = new StringBuilder();
        int i = 0;
        while (i < declaration.Length && declaration[i] != '(')
        {
            funcName.Append(declaration[i]);
            i++;
        }

        StringBuilder processedDeclaration = new StringBuilder(funcName.ToString());
        processedDeclaration.Append('(');
        bool firstVar = true;
        string possibleVars = "xyz";
        int numPossibleVars = possibleVars.Length;
        for(int j = 0; j < numPossibleVars; j++)
        {
            if (definition.Contains("" + possibleVars[j]))
            {
                if (firstVar)
                {
                    processedDeclaration.Append(possibleVars[j]);
                    firstVar = false;
                }
                else
                {
                    processedDeclaration.Append(',');//processedDeclaration.Append(','); possibleVars[j]);
                    processedDeclaration.Append(possibleVars[j]);
                }
            }
        }
        name = funcName.ToString();
        processedDeclaration.Append(')');
        if (firstVar) return name;
        return processedDeclaration.ToString();
    }

    /*
     * Checks for mistakes in definition and fix it
     */
    private string ProcessDefinition(string definition)
    {
        if (definition.Length == 0) definition = "0";

        //Check if a multiplication is omitted for parenthesis
        string aux = "";
        string symbols = "+-*^/()";
        string cut = definition;
        for(int i = 0; i < definition.Length-1; i++)
        {
            bool startsWithSubFunc = false;
            string subFunction = "";
            foreach(var n in FunctionManager.functions.Keys)
            {
                if (cut.StartsWith(n))
                {
                    startsWithSubFunc = true;
                    subFunction = n;
                    break;
                }
            }
            if (!startsWithSubFunc)
            {
                foreach (var n in FunctionNode.subFunctions)
                {
                    if (cut.StartsWith(n))
                    {
                        startsWithSubFunc = true;
                        subFunction = n;
                        break;
                    }
                }
            }

            if (startsWithSubFunc)
            {
                i += subFunction.Length;
                aux += subFunction + "(";
                cut = cut.Substring(subFunction.Length + (cut.Length > subFunction.Length ? 1 : 0));
            } else
            {
                if (i > 0 && definition[i] == '(' && !symbols.Contains("" + definition[i - 1])) aux += '*';
                aux += definition[i];
                if (definition[i] == ')' && !symbols.Contains("" + definition[i + 1])) aux += "*";
                cut = cut.Substring(1);
            }
        }

        aux += definition[definition.Length - 1];
        
        definition = aux;

        //Check is a multiplication is omitted between variables
        aux = "";
        string cutDef = definition;
        bool previousVar = false;
        bool previousNumber = false;
        const string numberSymbols = "0123456789.";
        while (cutDef.Length > 0)
        {
            bool isVariable = false;
            for (int j = 0; j < FunctionNode.variables.Count; j++)
            {
                string variable = FunctionNode.variables[j];
                if (cutDef.StartsWith(variable))
                {
                    isVariable = true;
                    if (previousVar || previousNumber)
                    {
                        aux += '*';
                    }
                    aux += variable;
                    previousNumber = false;
                    cutDef = cutDef.Substring(variable.Length);
                    if(cutDef.Length > 0 && numberSymbols.Contains("" + cutDef[0])) aux += '*';
                    break;
                }
                
            }
            previousVar = isVariable;
            
            if (!previousVar)
            {
                aux += cutDef[0];
                previousNumber = numberSymbols.Contains(""+cutDef[0]);
                cutDef = cutDef.Substring(1);
            }
        }
        definition = aux;

        return definition;
    }
}
