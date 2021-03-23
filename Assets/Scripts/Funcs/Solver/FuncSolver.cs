using UnityEngine;
using System;

namespace FuncSpace
{
    public class FuncSolver : IFuncSolver
    {
        public float Solve(ref Vector3 pos, Bytecode code, float[] memory)
        {
            SetPositionToMemory(ref pos, memory);
            SolveOperations(code, memory);
            return GetFinalResult(code, memory);
        }

        private void SetPositionToMemory(ref Vector3 pos, float[] memory)
        {
            memory[0] = 0;
            memory[1] = pos.x;
            memory[2] = pos.y;
            memory[3] = pos.z;
        }

        private float GetFinalResult(Bytecode code, float[] memory)
        {
            float finalResult = GetMemoryAt(code.resultIndex, memory);
            return finalResult;
        }

        private void SolveOperations(Bytecode code, float[] memory)
        {
            int i = 0;
            var operations = code.operations;
            int operationsLength = operations.Count;
            while (i < operationsLength)
            {
                float result = GetOperationResult(code, memory, ref i);
                int opResultIndex = operations[i];
                memory[opResultIndex] = result;
                i++;
            }
        }

        private float GetOperationResult(Bytecode code, float[] memory, ref int opIndex)
        {
            int op = code.operations[opIndex++];
            float v0 = GetMemoryAt(code.operations[opIndex++], memory);
            float v1 = GetMemoryAt(code.operations[opIndex++], memory);
            float result;

            if (OpIsSubfunction(op))
            {
                float v2 = GetMemoryAt(code.operations[opIndex++], memory);
                result = SolveSubFunction(op, v0, v1, v2);
            }
            else
                result = SolveOperator(op, v0, v1);

            //opIndex++;
            return result;
        }

        private bool OpIsSubfunction(int op)
        {
            return op > FuncGeneralInfo.MaxOperatorIndex - 1;
        }

        private float GetMemoryAt(int i, float[] memory)
        {
            return memory[Math.Abs(i)] * (i >= 0 ? 1f : -1f);
        }

        float ChangeSignIf(float val, bool condition)
        {
            return val * (-2 * (condition ? 1 : -1) + 1);
        }

        float SolveSubFunction(int op, float v0, float v1, float v2)
        {
            const int FUNC_COS = 0;
            const int FUNC_SIN = 1;
            const int FUNC_ABS = 2;
            int subFunction = op - FuncGeneralInfo.MaxOperatorIndex;
            switch (subFunction)
            {
                case FUNC_COS:
                    return Mathf.Cos(v0);
                case FUNC_SIN:
                    return Mathf.Sin(v0);
                case FUNC_ABS:
                    return Mathf.Abs(v0);
                default:
                    return 0.0f;
            }
        }

        
        float SolveOperator(int operation, float v0, float v1)
        {
            const int SUBTRACT = 0;
            const int ADD = 1;
            const int PRODUCT = 2;
            const int DIVISION = 3;
            const int POWER = 4;
            switch (operation)
            {
                case SUBTRACT:
                    return v0 - v1;
                case ADD:
                    return v0 + v1;
                case PRODUCT:
                    return v0 * v1;
                case DIVISION:
                    return v0 / v1;
                case POWER:
                    uint v1Int = (uint)v1;
                    bool v1IsEven = v1Int % 2 == 0;
                    bool v0IsPositive = v0 >= 0;
                    float p = Mathf.Pow(Mathf.Abs(v0), v1Int);
                    p = ChangeSignIf(p, !v0IsPositive && !v1IsEven);
                    return p;
                default:
                    return 0.0f;
            }
        }
    }
}