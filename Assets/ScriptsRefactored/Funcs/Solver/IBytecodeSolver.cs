using System.Collections;
using UnityEngine;

namespace FuncSpace
{
    public interface IBytecodeSolver
    {
        float Solve(Vector3 vec, Bytecode code, float[] auxMemory);
    }
}