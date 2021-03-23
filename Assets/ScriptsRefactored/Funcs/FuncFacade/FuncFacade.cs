﻿using System.Collections.Generic;
using System;

namespace FuncSpace
{
    public class FuncFacade : IFuncFacade
    {

        private IFuncFactory factory;
        private IFuncSolver bytecodeSolver;
        private IFunc selectedFunc;

        public event Action onChanged;
        public FuncFacade()
        {
            factory = new FuncFactory();
            bytecodeSolver = new FuncSolver();
            selectedFunc = factory.GetDummy();
        }

        #region selection

        public List<string> GetAllFuncNames()
        {
            List<string> allFuncs = new List<string>();
            factory.ForEachFuncName((name) =>
            {
                var func = factory.GetFunc(name);
                allFuncs.Add(func.ToString());
            });
            return allFuncs;
        }

        public string GetFuncByName(string name)
        {
            if (factory.IsFuncDefinedByUser(name))
                return factory.GetFunc(name).ToString();
            else
                return "";
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
            factory.CreateFunc(textFunc);
            onChanged?.Invoke();
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

        public float Solve(float x, float y, float z)
        {
            var memory = selectedFunc.BytecodeInfo.memory;
            return bytecodeSolver.Solve(x, y, z, selectedFunc.BytecodeInfo, memory);
        }

        public float Solve(float x, float y, float z, float[] memory)
        {
            return bytecodeSolver.Solve(x, y, z, selectedFunc.BytecodeInfo, memory);
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

        public int GetMaxOperatorIndex()
        {
            return FuncGeneralInfo.MaxOperatorIndex;
        }

        #endregion
    }
}

