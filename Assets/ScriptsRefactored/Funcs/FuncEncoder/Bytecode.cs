using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Bytecode
{
    public const int maxMemorySize = 256;
    public const int maxOperationsSize = 512;

    public List<int> operations;
    public float[] memory;
    public int resultIndex;

    public Bytecode()
    {
        operations = new List<int>();
        memory = new float[maxMemorySize];
        resultIndex = 0;
    }
}
