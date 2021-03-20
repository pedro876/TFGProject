using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FuncSpace
{
    public class FuncEncoder : IFuncEncoder
    {
        private IFuncFactory factory;
        
        private const int memoryStart = 4;
        Dictionary<IFuncNode, int> memoryNodes;
        Bytecode bytecode;
        int lastMemoryIndex;
        HashSet<string> subFuncsRecord;

        public FuncEncoder(IFuncFactory factory)
        {
            this.factory = factory;
            memoryNodes = new Dictionary<IFuncNode, int>();
            subFuncsRecord = new HashSet<string>();
        }

        public void Encode(IFunc func)
        {
            CreateBytecode(func);
            func.BytecodeInfo = bytecode;
            Reset();
        }

        private void CreateBytecode(IFunc func)
        {
            lastMemoryIndex = memoryStart;
            bytecode = new Bytecode();
            int[] variablesIndex = new int[] { 1, 2, 3 };

            DeepEncode(variablesIndex, func.RootNode);
            FillByteCodeMemory();

            bytecode.resultIndex = memoryNodes[func.RootNode];
        }

        private void Reset()
        {
            memoryNodes.Clear();
            subFuncsRecord.Clear();
            bytecode = null;
        }

        private void FillByteCodeMemory()
        {
            foreach(var nodeIdx in memoryNodes)
            {
                var node = nodeIdx.Key;
                var idx = nodeIdx.Value;
                if (node is ConstantNode)
                {
                    bytecode.memory[idx] = ((ConstantNode)node).GetValue();
                }
                else
                    bytecode.memory[idx] = 0f;
            }
        }

        private void DeepEncode(int[] varIdx, IFuncNode node)
        {
            if (lastMemoryIndex < Bytecode.maxMemorySize)
            {
                int memoryIndex = 0;

                if(node is LeafNode)
                    memoryIndex = DeepEncodeLeafNode(varIdx, node);
                else if(node is NonLeafNode)
                    memoryIndex = DeepEncodeNonLeafNode(varIdx, node);
                
                memoryNodes[node] = memoryIndex;
            }
        }

        private int DeepEncodeLeafNode(int[] varIdx, IFuncNode node)
        {
            int memoryIndex = 0;
            if (node is VariableNode)
                memoryIndex = DeepEncodeVariableNode(varIdx, node);
            else if (node is ConstantNode)
                memoryIndex = DeepEncodeConstantNode(varIdx, node);
            return memoryIndex;
        }

        private int DeepEncodeVariableNode(int[] varIdx, IFuncNode node)
        {
            int memoryIndex = 0;
            switch (((VariableNode)node).Variable)
            {
                case "x": memoryIndex = varIdx[0]; break;
                case "y": memoryIndex = varIdx[1]; break;
                case "z": memoryIndex = varIdx[2]; break;
            }
            return memoryIndex;
        }

        private int DeepEncodeConstantNode(int[] varIdx, IFuncNode node)
        {
            int memoryIndex = lastMemoryIndex;
            lastMemoryIndex++;
            return memoryIndex;
        }

        private int DeepEncodeNonLeafNode(int[] varIdx, IFuncNode node)
        {
            int memoryIndex = 0;
            DeepEncodeChildren(varIdx, (NonLeafNode)node);
            
            if (node is SubFuncNode)
                memoryIndex = DeepEncodeSubFunction(varIdx, (SubFuncNode)node);
            else if (node is OperatorNode)
                memoryIndex = DeepEncodeOperatorNode((OperatorNode)node);

            return memoryIndex;
        }

        private int DeepEncodeSubFunction(int[] varIdx, SubFuncNode node)
        {
            int memoryIndex = 0;
            var subFuncVarIdx = GetSubFuncVarIndexes(node);
            if (node is UserDefinedFuncNode)
                memoryIndex = DeepEncodeUserDefinedFuncNode(subFuncVarIdx, (UserDefinedFuncNode)node);
            else if (node is PredefinedFuncNode)
                memoryIndex = DeepEncodePredefinedFuncNode(subFuncVarIdx, (PredefinedFuncNode)node);
            return memoryIndex;
        }

        private int[] GetSubFuncVarIndexes(SubFuncNode node)
        {
            int[] subFuncVarIdx = new int[3];
            var children = node.GetChildren();
            int top = Math.Min(3, children.Count);
            for (int i = 0; i < top; i++)
            {
                subFuncVarIdx[i] = memoryNodes[children[i]];
            }
            return subFuncVarIdx;
        }

        private int DeepEncodeUserDefinedFuncNode(int[] varIdx, UserDefinedFuncNode node)
        {
            int memoryIndex = 0;
            IFunc subFunc = node.Func;
            if (!subFuncsRecord.Contains(subFunc.Name))
            {
                subFuncsRecord.Add(subFunc.Name);
                DeepEncode(varIdx, subFunc.RootNode);
                subFuncsRecord.Remove(subFunc.Name);
                memoryIndex = memoryNodes[subFunc.RootNode];
            }
            return memoryIndex;
        }

        private int DeepEncodePredefinedFuncNode(int[] varIdx, PredefinedFuncNode node)
        {
            int memoryIndex = 0;
            if ((Bytecode.maxOperationsSize - bytecode.operations.Count) >= 5)
            {
                memoryIndex = lastMemoryIndex;
                bytecode.operations.Add(GetPredefinedFuncIndex(node));
                bytecode.operations.Add(varIdx[0]);
                bytecode.operations.Add(varIdx[1]);
                bytecode.operations.Add(varIdx[2]);
                bytecode.operations.Add(memoryIndex);
                lastMemoryIndex++;
            }
            return memoryIndex;
        }

        private int DeepEncodeOperatorNode(OperatorNode node)
        {
            int memoryIndex = 0;
            if ((Bytecode.maxOperationsSize - bytecode.operations.Count) >= 4)
            {
                memoryIndex = lastMemoryIndex;

                bytecode.operations.Add(GetOperationIndex(node));
                bytecode.operations.Add(memoryNodes[node.LeftChild]);
                bytecode.operations.Add(memoryNodes[node.RightChild]);
                bytecode.operations.Add(memoryIndex);

                lastMemoryIndex++;
            }
            return memoryIndex;
        }

        private void DeepEncodeChildren(int[] varIdx, NonLeafNode node)
        {
            foreach(var child in node.GetChildren())
            {
                DeepEncode(varIdx, child);
            }
        }

        private int GetPredefinedFuncIndex(PredefinedFuncNode node)
        {
            return factory.Operators.Count + factory.PredefinedFuncs.IndexOf(node.FunctionName);
        }
        private int GetOperationIndex(OperatorNode node)
        {
            return factory.Operators.IndexOf(node.OperatorSymbol);
        }
    }
}