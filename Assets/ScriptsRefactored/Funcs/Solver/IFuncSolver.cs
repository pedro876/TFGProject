using System.Collections;

namespace FuncSpace
{
    public interface IFuncSolver
    {
        float Solve(float x, float y, float z, Bytecode code, float[] auxMemory);
    }
}