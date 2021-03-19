using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuncSpace
{
    public class FuncFactory
    {
        private static FuncFactory instance = null;
        public static FuncFactory Instance
        {
            get
            {
                if (instance == null)
                    instance = new FuncFactory();
                return instance; ;
            }
        }

        private FuncReader reader;
        private FuncInterpreter interpreter;

        private Dictionary<string, IFunc> userDefinedFuncs;
        private HashSet<string> allFuncNames;
        private readonly HashSet<string> variables;
        private readonly List<string> operators;
        private readonly Dictionary<string, int> operatorPriorities;
        private readonly IFunc dummyFunc;

        public FuncFactory()
        {
            //provider = FuncProvider.Instance;
            reader = new FuncReader();
            interpreter = new FuncInterpreter();

            userDefinedFuncs = new Dictionary<string, FuncSpace.IFunc>();
            dummyFunc = new DummyFunc();
            allFuncNames = new HashSet<string>()
            {
                "cos", "sin", "abs"
            };
            variables = new HashSet<string>()
            {
                "x","y","z"
            };
            operators = new List<string>()
            {
                "-", "+", "*", "/", "^",
            };
            operatorPriorities = new Dictionary<string, int>()
            {
                { "-", 0 },
                { "+", 0 },
                { "*", 1 },
                { "/", 1 },
                { "^", 1 },
            };
        }

        public IFunc DummyFunc => dummyFunc;
        public int MaxOperatorIndex => operators.Count;
        public HashSet<string> AllFuncNames => allFuncNames;
        public HashSet<string> Variables => variables;
        public List<string> Operators => operators;

        public IFunc CreateFunc(string textFunc)
        {
            IFunc func = CreateFuncFromText(textFunc);
            AddFunc(func);
            return func;
        }

        private IFunc CreateFuncFromText(string textFunc)
        {
            IFunc func = new Func();
            IFunc processedFunc = reader.ExtractOriginalFuncInfo(textFunc, func);
            interpreter.CreateNodeTreeForFunc(processedFunc);
            processedFunc = reader.ExtractFinalFuncInfo(func);
            return func;
        }

        private void AddFunc(IFunc func)
        {
            userDefinedFuncs[func.GetName()] = func;
            allFuncNames.Add(func.GetName());
            CheckIfPreviousFuncsMustBeUpdated(func);
        }

        private void CheckIfPreviousFuncsMustBeUpdated(IFunc func)
        {
            foreach (var otherFunc in userDefinedFuncs.Values)
            {
                if (otherFunc != func)
                {
                    string definition = otherFunc.GetOriginalDefinition();
                    bool mustBeUpdated = definition.Contains(func.GetName()) && !func.GetSubfunctions().Contains(func.GetName());
                    if (mustBeUpdated)
                        UpdateFunc(otherFunc);
                }
            }
        }

        private void UpdateFunc(IFunc func)
        {
            interpreter.CreateNodeTreeForFunc(func);
            reader.ExtractFinalFuncInfo(func);
        }

        public bool ContainsFunc(string name)
        {
            return userDefinedFuncs.ContainsKey(name);
        }

        public void RemoveFunc(string name)
        {
            userDefinedFuncs.Remove(name);
            allFuncNames.Remove(name);
        }

        public IFunc GetFunc(string name)
        {
            if (ContainsFunc(name))
            {
                return userDefinedFuncs[name];
            }
            else
            {
                return dummyFunc;
            }
        }

        public bool IsFuncUserDefined(string name)
        {
            return userDefinedFuncs.ContainsKey(name);
        }
    }
}