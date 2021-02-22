using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FunctionReader : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;
    [SerializeField] TextMeshProUGUI processedFuncDisplay;

    string funcBytecode = "";
    Function currentFunction;

    private void Start()
    {
        //ProcessFunc(inputField.text);
        StartCoroutine(UpdateFuncCoroutine());
    }

    IEnumerator UpdateFuncCoroutine()
    {
        while (true)
        {
            Function func = ProcessFunc(inputField.text);
            processedFuncDisplay.text = func != null ? func.ToString() : "invalid func";
            yield return new WaitForSeconds(0.3f);

        }
        
    }

    /*
     * Processes a function based on human comprehensible text
     */
    private Function ProcessFunc(string funcText)
    {
        funcText = funcText.ToLower();
        string rawFunc = GetRawFunc(ref funcText);
        string[] parts = rawFunc.Split('=');
        if (parts.Length < 2) return null;
        string name;
        int displacement;
        string declaration = ProcessDeclaration(parts[0], ref parts[1], out name, out displacement);
        string definition = ProcessDefinition(parts[1]);

        Function func = new Function(name, declaration, definition);
        inputField.text = declaration +" = "+ parts[1];
        if(displacement > 0)
        {
            inputField.selectionAnchorPosition += displacement;
            inputField.selectionFocusPosition += displacement;
        } /*else if (displacement < 0)
        {
            Debug.Log("displacement: "+displacement);
            inputField.selectionFocusPosition += displacement;
            inputField.selectionAnchorPosition += displacement;
        }*/
        
        return func;
    }


    /*
     * Check if some coordinate variable is missing in the declaration and fix it
     */
    private string ProcessDeclaration(string declaration, ref string definition, out string name, out int displacement)
    {
        string originalDeclaration = declaration;
        string funcName = "";
        int i = 0;
        while (i < declaration.Length && declaration[i] != '(')
        {
            funcName += declaration[i];
            i++;
        }
        declaration = funcName+'(';
        bool firstVar = true;
        string possibleVars = "xyz";
        int numPossibleVars = possibleVars.Length;
        for(int j = 0; j < numPossibleVars; j++)
        {
            if (definition.Contains("" + possibleVars[j]))
            {
                if (firstVar)
                {
                    declaration += possibleVars[j];
                    firstVar = false;
                }
                else declaration += "," + possibleVars[j];
            }
        }
        declaration += ')';
        name = funcName;
        displacement = declaration.Length - originalDeclaration.Length;
        return declaration;
    }

    /*
     * Checks for mistakes in definition and fix it
     */
    private string ProcessDefinition(string definition)
    {
        if (definition.Length == 0) definition = "0";

        //Check if a multiplication is omitted for parenthesis
        string aux = ""+definition[0];
        string symbols = "+-*^/";
        for(int i = 1; i < definition.Length-1; i++)
        {
            if (definition[i] == '(' && !symbols.Contains("" + definition[i - 1])) aux += '*';
            aux += definition[i];
            if (definition[i] == ')' && !symbols.Contains("" + definition[i + 1])) aux += "*";
        }
        if(definition.Length > 1)
        {
            aux += definition[definition.Length - 1];
        }
        
        definition = aux;

        //Check is a multiplication is omitted between variables
        aux = "";
        string cutDef = definition;
        bool previousVar = false;
        while(cutDef.Length > 0)
        {
            bool isVariable = false;
            for (int j = 0; j < FuncNode.variables.Count; j++)
            {
                string variable = FuncNode.variables[j];
                if (cutDef.StartsWith(variable))
                {
                    isVariable = true;
                    if (previousVar)
                    {
                        aux += "*";
                    }
                    aux += variable;
                    cutDef = cutDef.Substring(variable.Length);
                    break;
                }
                
            }
            previousVar = isVariable;
            if (!previousVar)
            {
                aux += cutDef[0];
                cutDef = cutDef.Substring(1);
            }
        }
        definition = aux;

        return definition;
    }


    /*
     * Remove all whitespaces from the function
     */
    private string GetRawFunc(ref string raw)
    {
        string rawFunc = "";
        int size = raw.Length;
        for (int i = 0; i < size; i++) if (!raw[i].Equals(' ')) rawFunc += raw[i];
        return rawFunc;
    }
}
