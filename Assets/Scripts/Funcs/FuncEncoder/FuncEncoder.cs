using System.Collections.Generic;
using System;

namespace FuncSpace
{
    public class FuncEncoder : IFuncEncoder
    {
        private const int memoryStart = 4;

        private IFuncFactory factory;
        private Bytecode bytecode;
        
        private Dictionary<IFuncNode, int> memoryNodes;
        private HashSet<string> subFuncsRecord;
        private int lastMemoryIndex;

        public FuncEncoder(IFuncFactory factory)
        {
            this.factory = factory;
            memoryNodes = new Dictionary<IFuncNode, int>();
            subFuncsRecord = new HashSet<string>();
        }

        public Bytecode Encode(IFuncNode rootNode)
        {
            Bytecode code = CreateBytecode(rootNode);
            Reset();
            return code;
        }

        private Bytecode CreateBytecode(IFuncNode rootNode)
        {
            lastMemoryIndex = memoryStart;
            bytecode = new Bytecode();
            int[] variablesIndex = new int[] { 1, 2, 3 };

            DeepEncode(variablesIndex, rootNode);
            FillByteCodeMemory();

            bytecode.resultIndex = memoryNodes[rootNode];
            bytecode.maxMemoryIndexUsed = lastMemoryIndex;
            return bytecode;
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
                if (node is ConstantNode cNode)
                    bytecode.memory[idx] = cNode.GetValue();
                else
                    bytecode.memory[idx] = 0f;
            }
        }

        #region DeepEnconde

        private void DeepEncode(int[] varIdx, IFuncNode node)
        {
            if (lastMemoryIndex < Bytecode.maxMemorySize)
            {
                int memoryIndex = 0;

                if (node is LeafNode leafNode)
                    memoryIndex = DeepEncodeLeafNode(varIdx, leafNode);
                else if (node is NonLeafNode nonLeafNode)
                    memoryIndex = DeepEncodeNonLeafNode(varIdx, nonLeafNode);
                
                memoryNodes[node] = memoryIndex;
            }
        }

        private int DeepEncodeLeafNode(int[] varIdx, LeafNode node)
        {
            int memoryIndex = 0;
            if (node is VariableNode vNode)
                memoryIndex = DeepEncodeVariableNode(varIdx, vNode);
            else if (node is ConstantNode)
                memoryIndex = DeepEncodeConstantNode();
            return memoryIndex;
        }

        private int DeepEncodeVariableNode(int[] varIdx, VariableNode node)
        {
            int memoryIndex = 0;
            switch (node.Variable)
            {
                case "x": memoryIndex = varIdx[0]; break;
                case "y": memoryIndex = varIdx[1]; break;
                case "z": memoryIndex = varIdx[2]; break;
            }
            return memoryIndex;
        }

        private int DeepEncodeConstantNode()
        {
            int memoryIndex = lastMemoryIndex;
            lastMemoryIndex++;
            return memoryIndex;
        }

        private int DeepEncodeNonLeafNode(int[] varIdx, NonLeafNode node)
        {
            int memoryIndex = 0;
            DeepEncodeChildren(varIdx, node);
            
            if (node is SubFuncNode subFuncNode)
                memoryIndex = DeepEncodeSubFunction(subFuncNode);
            else if (node is OperatorNode opNode)
                memoryIndex = DeepEncodeOperatorNode(opNode);

            return memoryIndex;
        }

        private int DeepEncodeSubFunction(SubFuncNode node)
        {
            int memoryIndex = 0;
            var subFuncVarIdx = GetSubFuncVarIndexes(node);
            if (node is UserDefinedFuncNode userDefinedNode)
                memoryIndex = DeepEncodeUserDefinedFuncNode(subFuncVarIdx, userDefinedNode);
            else if (node is PredefinedFuncNode predefinedNode)
                memoryIndex = DeepEncodePredefinedFuncNode(subFuncVarIdx, predefinedNode);
            return memoryIndex;
        }

        private int[] GetSubFuncVarIndexes(SubFuncNode node)
        {
            int[] subFuncVarIdx = new int[3];
            var children = node.GetChildren();
            int top = Math.Min(3, children.Count);
            for (int i = 0; i < top; i++)
                subFuncVarIdx[i] = memoryNodes[children[i]];
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
                memoryIndex = FindSuitablePosition(node);
                bytecode.operations.Add(GetPredefinedFuncIndex(node));
                bytecode.operations.Add(varIdx[0]);
                bytecode.operations.Add(varIdx[1]);
                bytecode.operations.Add(varIdx[2]);
                bytecode.operations.Add(memoryIndex);
                //lastMemoryIndex++;
            }
            return memoryIndex;
        }

        private int DeepEncodeOperatorNode(OperatorNode node)
        {
            int memoryIndex = 0;
            if ((Bytecode.maxOperationsSize - bytecode.operations.Count) >= 4)
            {
                memoryIndex = FindSuitablePosition(node);

                bytecode.operations.Add(GetOperationIndex(node));
                bytecode.operations.Add(memoryNodes[node.LeftChild]);
                bytecode.operations.Add(memoryNodes[node.RightChild]);
                bytecode.operations.Add(memoryIndex);

                //lastMemoryIndex++;
            }
            return memoryIndex;
        }

        private int FindSuitablePosition(NonLeafNode parent)
        {
            Queue<IFuncNode> children = new Queue<IFuncNode>();

            foreach (var child in parent.GetChildren())
                children.Enqueue(child);

            while(children.Count > 0)
            {
                var child = children.Dequeue();
                if(!(child is LeafNode) && memoryNodes.ContainsKey(child))
                {
                    return memoryNodes[child];
                }
                if(child is NonLeafNode nonLeafNode)
                {
                    foreach (var subChild in nonLeafNode.GetChildren())
                        children.Enqueue(subChild);
                }
            }
            int memoryIndex = lastMemoryIndex;
            lastMemoryIndex++;
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
            return FuncGeneralInfo.NumOperators + factory.IndexOfPredefinedFunc(node.FunctionName);
        }
        private int GetOperationIndex(OperatorNode node)
        {
            return FuncGeneralInfo.IndexOfOperator(node.OperatorSymbol);
        }

        #endregion
    }
}