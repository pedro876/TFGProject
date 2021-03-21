using System.Collections;
using UnityEngine;

namespace FuncSpace
{
    public interface IFuncSolver
    {
        float Solve(Vector3 vec, IFunc func);
    }
}