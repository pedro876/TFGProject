using System.Collections;
using UnityEngine;

namespace FuncSpace
{
    public interface IFuncSolver
    {
        float Solve(ref Vector3 pos, Bytecode code, float[] auxMemory);
    }
}