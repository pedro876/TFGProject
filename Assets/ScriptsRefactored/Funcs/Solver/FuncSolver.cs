using System.Collections;
using UnityEngine;

namespace FuncSpace
{
    public class FuncSolver : IFuncSolver
    {
        public float Solve(Vector3 vec, IFunc func)
        {
            return func.Solve(vec.x, vec.y, vec.z);
        }
    }
}