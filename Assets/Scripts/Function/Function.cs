using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Function
{
    public FunctionNode rootNode;
    public string name;
    private string originalDeclaration;
    private string finalDeclaration;
    public List<string> variables;
    public string originalDefinition;
    private string processedDefinition;
    private string finalDefinition;
    private int[] bytecode;
    private int memorySize;
    private Dictionary<FunctionNode, int> memoryNodes;
    private int maxOperatorIndex;
    public int numOperations;
    public int resultIndex;

    public const int maxMemorySize = 64;
    public const int maxOperationsSize = 256;

    public Function(string name, string declaration,string originalDefinition, string definition)
    {
        variables = new List<string>();
        SetData(name,  declaration, originalDefinition, definition);
    }

    public Function Clone()
    {
        return new Function(name, originalDeclaration, originalDefinition, finalDefinition);
    }

    public float Solve(float x = 0f, float y = 0f, float z = 0f)
    {
        float result = 0f;
        if (rootNode != null) result = rootNode.Solve(x, y, z);
        return result;
    }

    public bool IsMass(ref Vector3 p, float[] memory)
    {
        bool outOfRegion = ViewController.IsOutOfRegion(ref p);
        if (outOfRegion) return false;
        float eval = SolveByteCode(memory, p.x, p.y, p.z);
        //float eval = Solve(p.x, p.y, p.z);
        return VolumeInterpreter.Interpretate(ref p, eval);
    }

    #region data processing

    private void SetData(string name, string declaration, string originalDefinition, string definition)
    {
        this.originalDefinition = originalDefinition;
        this.processedDefinition = definition;
        this.name = name;

        rootNode = new FunctionNode(null);
        this.originalDeclaration = declaration;
        rootNode.ProcessFunc(definition);
        finalDefinition = rootNode.ToString();

        variables = new List<string>();
        string[] parts = declaration.Split('(');
        if(parts.Length > 1)
        {
            string rawVars = declaration.Split('(')[1];
            rawVars = rawVars.Substring(0, rawVars.Length - 1);
            string[] aux = rawVars.Split(',');
            finalDeclaration = name + "(";
            for (int i = 0; i < aux.Length; i++)
            {
                variables.Add(aux[i]);
                finalDeclaration += aux[i];
                if (i < aux.Length - 1) finalDeclaration += ",";
            }

            if (variables.Count == 0) finalDeclaration = name;
            else finalDeclaration += ")";
        } else
        {
            finalDeclaration = name;
        }
        CreateByteCode();
    }

    public void ResetData()
    {
        SetData(name, originalDeclaration, originalDefinition, processedDefinition);
    }

    public bool UsesSubFunction(string subFunc)
    {
        return originalDefinition.Contains(subFunc);
    }

    #endregion

    #region bytecode

    private void CreateByteCode()
    {
        maxOperatorIndex = FunctionNode.GetMaxOperatorIndex;
        memoryNodes = new Dictionary<FunctionNode, int>();
        List<int> operations = new List<int>();
        HashSet<string> subFuncs = new HashSet<string>();
        memorySize = rootNode.CalculateBytecode(memoryNodes, operations, subFuncs, 4, maxMemorySize);
        resultIndex =memoryNodes[rootNode];
        int[] arr = operations.ToArray();

        int max = Math.Min(arr.Length, maxOperationsSize);
        List<int> aux = new List<int>();
        numOperations = 0;
        int i = 0;
        while(i < max)
        {
            int op = arr[i];
            int spaceRequired = (op > maxOperatorIndex - 1) ? 5 : 4;
            if (i + spaceRequired > maxOperationsSize) break;
            else
            {
                numOperations++;
                for (int j = 0; j < spaceRequired; j++)
                {
                    aux.Add(arr[i + j]);
                }
                i += spaceRequired;
            }
        }
        bytecode = aux.ToArray();
    }

    public int[] GetBytecode()
    {
        return bytecode;
    }

    public float[] CreateBytecodeMemory()
    {
        float[] memory = new float[memorySize];

        foreach(KeyValuePair<FunctionNode, int> node in memoryNodes)
        {
            if (node.Key.IsFloat) memory[node.Value] = node.Key.Value;
        }

        return memory;
    }

    public float SolveByteCode(float[] memory, float x = 0f, float y = 0f, float z = 0f)
    {
        memory[0] = 0;
        memory[1] = x;
        memory[2] = y;
        memory[3] = z;

        int i = 0;
        int length = bytecode.Length;
        while (i < length)
        {
            int op = bytecode[i];
            i++;
            float v0 = memory[Math.Abs(bytecode[i])] * (bytecode[i] >= 0 ? 1f : -1f);
            i++;
            float v1 = memory[Math.Abs(bytecode[i])] * (bytecode[i] >= 0 ? 1f : -1f);

            float result;
            if (op > maxOperatorIndex-1) //is sub function
            {
                i++;
                float v2 = memory[Math.Abs(bytecode[i])] * (bytecode[i] >= 0 ? 1f : -1f);
                result = FunctionNode.SolveSubFunction(op-maxOperatorIndex, v0, v1, v2);
            }
            else //is operator
            {
                result = FunctionNode.SolveOperator(op, v0, v1);
            }
            i++;
            int rIdx = bytecode[i];
            memory[rIdx] = result;
            i++;
        }
        int absIdx = Math.Abs(resultIndex);
        return memory[absIdx] * (resultIndex >= 0 ? 1f : -1f);
    }

    #endregion

    public override string ToString()
    {
        return finalDeclaration + " = " + finalDefinition;
    }

    public string ToOriginalString()
    {
        return originalDeclaration + " = " + originalDeclaration;
    }

    public bool Equals(Function f)
    {
        return finalDefinition.Equals(f.finalDefinition);
    }
}
