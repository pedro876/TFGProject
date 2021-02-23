using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Function
{
    public FunctionNode rootNode;
    public string name;
    public string declaration;
    public List<string> variables;
    public string originalDefinition;
    public string finalDefinition;
    

    public Function(string name, string declaration, string definition)
    {
        variables = new List<string>();
        SetData(name, declaration, definition);
    }

    public Function Clone()
    {
        return new Function(name, declaration, finalDefinition);
    }

    public float Solve(float x = 0f, float y = 0f, float z = 0f)
    {
        float result = 0f;
        if (rootNode != null) result = rootNode.Solve(x, y, z);
        return result;
    }

    public void SetData(string name, string declaration, string definition)
    {
        originalDefinition = definition;
        this.name = name;

        rootNode = new FunctionNode(null);
        this.declaration = declaration;
        rootNode.ProcessFunc(definition);
        finalDefinition = rootNode.ToString();

        variables = new List<string>();
        string rawVars = declaration.Split('(')[1];
        rawVars = rawVars.Substring(0, rawVars.Length - 1);
        string[] aux = rawVars.Split(',');
        foreach (string s in aux) variables.Add(s);
    }

    public override string ToString()
    {
        return declaration + " = " + finalDefinition;
    }
}
