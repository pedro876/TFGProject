using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Function
{
    public FuncNode rootNode;
    public string name;
    public string declaration;
    public string originalDefinition;
    public string finalDefinition;

    public Function(string name, string declaration, string definition)
    {
        SetData(name, declaration, definition);
    }

    public void SetData(string name, string declaration, string definition)
    {
        rootNode = new FuncNode(null);
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
