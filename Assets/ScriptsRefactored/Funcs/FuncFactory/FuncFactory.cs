using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuncSpace
{
    public class FuncFactory : IFuncFactory
    {
        /*private static FuncFactory instance = null;
        public static FuncFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FuncFactory();
                    instance.Init();
                }
                    
                return instance; ;
            }
        }
        */
        private IFuncReader reader;
        private IFuncInterpreter interpreter;

        private Dictionary<string, IFunc> userDefinedFuncs;
        private HashSet<string> allFuncNames;
        private HashSet<string> variables;
        private List<string> operators;
        private Dictionary<string, int> operatorPriorities;
        private IFunc dummyFunc;

        public FuncFactory()
        {
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
                { "-", 0 }, { "+", 0 },
                { "*", 1 }, { "/", 1 }, { "^", 1 },
            };
            userDefinedFuncs = new Dictionary<string, FuncSpace.IFunc>();
            reader = new FuncReader(factory: this);
            interpreter = new FuncInterpreter(factory: this);
            dummyFunc = CreateFunc("dummy(x) = x");
        }

        public IFunc DummyFunc => dummyFunc;
        public int MaxOperatorIndex => operators.Count;
        public HashSet<string> AllFuncNames => allFuncNames;
        public HashSet<string> Variables => variables;
        public List<string> Operators => operators;
        
        public int GetOperatorPriority(string op)
        {
            if (operatorPriorities.ContainsKey(op))
            {
                return operatorPriorities[op];
            }
            else
                return -1;
        }

        public IFunc CreateFunc(string textFunc)
        {
            IFunc func = CreateFuncFromText(textFunc);
            AddFunc(func);
            return func;
        }

        private IFunc CreateFuncFromText(string textFunc)
        {
            IFunc func = CreateFuncIfDidntExist(textFunc);
            reader.ExtractOriginalFuncInfo(textFunc, func);
            interpreter.CreateNodeTreeForFunc(func);
            reader.ExtractFinalFuncInfo(func);
            return func;
        }

        private IFunc CreateFuncIfDidntExist(string textFunc)
        {
            IFunc func = new Func();
            reader.ExtractOriginalFuncInfo(textFunc, func);
            if (ContainsFunc(func.Name))
            {
                func = GetFunc(func.Name);
            }
            return func;
        }

        private void AddFunc(IFunc func)
        {
            userDefinedFuncs[func.Name] = func;
            allFuncNames.Add(func.Name);
            UpdateCrossReferences(func);
        }

        private void UpdateCrossReferences(IFunc func)
        {
            foreach (var otherFunc in userDefinedFuncs.Values)
            {
                if (otherFunc != func)
                {
                    string definition = otherFunc.OriginalDefinition;
                    bool mustBeUpdated = definition.Contains(func.Name);/* && !func.Subfunctions.Contains(func.Name);*/
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

        public void RemoveAllFuncs()
        {
            List<string> toRemove = new List<string>();
            foreach(string func in userDefinedFuncs.Keys)
            {
                toRemove.Add(func);
            }
            foreach(string func in toRemove)
            {
                RemoveFunc(func);
            }
        }

        public void RemoveFunc(string name)
        {
            if (ContainsFunc(name))
            {
                IFunc func = userDefinedFuncs[name];
                if(func != dummyFunc)
                {
                    userDefinedFuncs.Remove(name);
                    allFuncNames.Remove(name);
                    RemoveCrossReferences(func);
                }
            }
        }

        private void RemoveCrossReferences(IFunc func)
        {
            foreach (var otherFunc in userDefinedFuncs.Values)
            {
                string definition = otherFunc.OriginalDefinition;
                if (otherFunc.Subfunctions.Contains(func.Name))
                {
                    UpdateFunc(otherFunc);
                }
            }
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
    }
}