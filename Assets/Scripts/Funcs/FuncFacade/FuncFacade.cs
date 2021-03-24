using System.Collections.Generic;
using System;
using UnityEngine;

namespace FuncSpace
{
    public class FuncFacade : IFuncFacade
    {

        private IFuncFactory factory;
        private IFuncSolver bytecodeSolver;
        private IFunc selectedFunc;
        private INoise noise;

        public event Action onChanged;
        public FuncFacade()
        {
            noise = new Noise();
            factory = new FuncFactory();
            AddDefaultFuncs();
            bytecodeSolver = new FuncSolver(noise);
            selectedFunc = factory.GetDummy();
        }

        private void AddDefaultFuncs()
        {
            //factory.CreateFunc("f(x) = x");
            //SelectFunc("f");
        }

        #region selection

        public List<string> GetAllFuncNames()
        {
            List<string> allFuncs = new List<string>();
            factory.ForEachUserDefinedFuncName((name) =>
            {
                allFuncs.Add(name);
            });
            return allFuncs;
        }

        public string GetFuncByName(string name)
        {
            if (factory.IsFuncDefinedByUser(name))
            {
                return factory.GetFunc(name).ToString();
            }
            else
            {
                return "";
            }
        }

        public string GetSelectedFunc()
        {
            return selectedFunc.ToString();
        }

        public string GetSelectedFuncName()
        {
            return selectedFunc.Name;
        }

        public bool SelectFunc(string funcName)
        {
            if (factory.IsFuncDefinedByUser(funcName))
            {
                selectedFunc = factory.GetFunc(funcName);
                onChanged?.Invoke();
                return true;
            }
            else
                return false;
        }

        public bool IsFuncSelected(string funcName)
        {
            return selectedFunc.Name.Equals(funcName);
        }

        public bool SelectedFuncUsesVariable(string variable)
        {
            return selectedFunc.Variables.Contains(variable);
        }

        #endregion

        #region creation and removal

        public void CreateFunc(string textFunc)
        {
            if (!textFunc.Trim().Equals(""))
            {
                factory.CreateFunc(textFunc);
                onChanged?.Invoke();
            }
        }

        public bool RemoveFunc(string funcName)
        {
            if (factory.IsFuncDefinedByUser(funcName))
            {
                if (selectedFunc.Name.Equals(funcName))
                    selectedFunc = factory.GetDummy();

                factory.RemoveFunc(funcName);
                onChanged?.Invoke();
                return true;
            }
            else
                return false;
        }

        public void Reset()
        {
            factory.RemoveAllFuncs();
            selectedFunc = factory.GetDummy();
            onChanged?.Invoke();
        }

        #endregion

        #region Solve

        public float Solve(ref Vector3 pos)
        {
            var memory = selectedFunc.BytecodeInfo.memory;
            return bytecodeSolver.Solve(ref pos, selectedFunc.BytecodeInfo, memory);
        }

        public float Solve(ref Vector3 pos, float[] memory)
        {
            return bytecodeSolver.Solve(ref pos, selectedFunc.BytecodeInfo, memory);
        }

        public float[] GetBytecodeMemCopy()
        {
            float[] copy = new float[Bytecode.maxMemorySize];
            for (int i = 0; i < Bytecode.maxMemorySize; i++)
                copy[i] = selectedFunc.BytecodeInfo.memory[i];
            return copy;
        }

        public List<int> GetBytecodeOperations()
        {
            return selectedFunc.BytecodeInfo.operations;
        }

        public int GetBytecodeResultIndex()
        {
            return selectedFunc.BytecodeInfo.resultIndex;
        }

        public int GetBytecodeMaxMemoryIndex()
        {
            return selectedFunc.BytecodeInfo.maxMemoryIndexUsed;
        }

        public int GetMaxOperatorIndex()
        {
            return FuncGeneralInfo.MaxOperatorIndex;
        }

        public int GetMaxMemorySize()
        {
            return Bytecode.maxMemorySize;
        }

        public int GetMaxOperationsSize()
        {
            return Bytecode.maxOperationsSize;
        }

        /*public float GetAuxValueForRandom() => noise.GetAuxValueForRandom();
        public Vector2 GetAuxVec2ForRandom() => noise.GetAuxVec2ForRandom();
        public Vector3 GetAuxVec3ForRandom() => noise.GetAuxVec3ForRandom();*/
        public int GetRandomStreamSize() => noise.GetStreamSize();
        public float[] GetRandomStream() => noise.GetRandomStream();

        #endregion
    }
}

