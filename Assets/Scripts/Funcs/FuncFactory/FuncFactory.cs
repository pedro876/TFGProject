using System.Collections.Generic;
using System;

namespace FuncSpace
{
    public class FuncFactory : IFuncFactory
    {
        private IFuncReader reader;
        private IFuncInterpreter interpreter;
        private IFuncSimplifier simplifier;
        private IFuncEncoder encoder;

        private Dictionary<string, IFunc> userDefinedFuncs;
        private HashSet<string> allFuncNames;
        private List<string> predefinedFuncs;
        private IFunc dummyFunc;

        public FuncFactory()
        {
            predefinedFuncs = new List<string>()
            {
                "cos", "sin", "abs", "rnd","rnd2", "rnd3", "round", "voxel", "perlin", "perlin2", "perlin3", "mag"
            };
            allFuncNames = new HashSet<string>();
            foreach (var f in predefinedFuncs)
            {
                allFuncNames.Add(f);
            }
            userDefinedFuncs = new Dictionary<string, FuncSpace.IFunc>();
            reader = new FuncReader(factory: this);
            interpreter = new FuncInterpreter(factory: this);
            simplifier = new FuncSimplifier();
            encoder = new FuncEncoder(factory:this);
            dummyFunc = CreateFunc("f(x) = cos(x) +cos(y)");
        }

        public IFunc GetDummy()
        {
            return dummyFunc;
        }
        public void ForEachFuncName(Action<string> action)
        {
            foreach(var funcName in allFuncNames)
            {
                action(funcName);
            }
        }

        public void ForEachUserDefinedFuncName(Action<string> action)
        {
            foreach(var funcName in userDefinedFuncs.Keys)
            {
                action(funcName);
            }
        }

        public int IndexOfPredefinedFunc(string funcName)
        {
            return predefinedFuncs.IndexOf(funcName);
        }

        #region CreateFunc

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
            UpdateFunc(func);
            return func;
        }
        private IFunc CreateFuncIfDidntExist(string textFunc)
        {
            IFunc func = new Func();
            reader.ExtractOriginalFuncInfo(textFunc, func);
            if (IsFuncDefinedByUser(func.Name))
            {
                func = GetFunc(func.Name);
            }
            return func;
        }

        private void UpdateFunc(IFunc func)
        {
            func.RootNode = interpreter.CreateNodeTreeForFunc(func.OriginalDefinition, func.Name);
            func.RootNode = simplifier.SimplifyFuncFromRoot(func.RootNode);
            reader.ExtractFinalFuncInfo(func);
            func.BytecodeInfo = encoder.Encode(func.RootNode);
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
        
        

        #endregion

        #region RemoveFuncs

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
            if (IsFuncDefinedByUser(name))
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

        #endregion

        public IFunc GetFunc(string name)
        {
            if (IsFuncDefinedByUser(name))
            {
                return userDefinedFuncs[name];
            }
            else
            {
                return dummyFunc;
            }
        }

        public bool IsFuncDefinedByUser(string name)
        {
            return userDefinedFuncs.ContainsKey(name);
        }
    }
}