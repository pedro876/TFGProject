using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Function
{
    public FunctionNode rootNode;
    public string name;
    public string declaration;
    public string originalDefinition;
    public string finalDefinition;

    public Function(string name, string declaration, string definition)
    {
        SetData(name, declaration, definition);
    }

    public Function Clone()
    {
        return new Function(name, declaration, finalDefinition);
    }

    public void SetData(string name, string declaration, string definition)
    {
        rootNode = new FunctionNode(null);
        this.declaration = declaration;
        rootNode.ProcessFunc(definition);
        originalDefinition = definition;
        finalDefinition = rootNode.ToString();
    }

    public override string ToString()
    {
        return declaration + " = " + finalDefinition;
    }
}
