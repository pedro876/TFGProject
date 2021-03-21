using System.Collections;
using UnityEngine;

namespace FuncSpace
{
    public class FuncSimplifier : IFuncSimplifier
    {
        public void SimplifyFunc(IFunc func)
        {
            func.RootNode = DeepSimplify(func.RootNode);
        }

        #region DeepSimplify

        private IFuncNode DeepSimplify(IFuncNode node)
        {
            if(node is NonLeafNode nonLeafNode)
            {
                var children = nonLeafNode.GetChildren();
                for(int i = 0; i < children.Count; i++)
                {
                    children[i] = DeepSimplify(children[i]);
                }
            }
            return SimplifyNode(node);
        }

        #endregion

        #region AbstractNodeSimplify

        private IFuncNode SimplifyNode(IFuncNode node)
        {
            IFuncNode simplifiedNode = node;
            if(node is OperatorNode opNode)
            {
                simplifiedNode = SimplifyOp(opNode);
                simplifiedNode.Parent = node.Parent;
            }
            return simplifiedNode;
        }

        private IFuncNode SimplifyOp(OperatorNode opNode)
        {
            IFuncNode simplifiedNode = opNode;
            if (opNode.LeftChild is ConstantNode && opNode.RightChild is ConstantNode)
            {
                float result = opNode.Solve(0, 0, 0);
                simplifiedNode = new ConstantNode(result);
            }
            else if (opNode is OperatorMulNode)
                simplifiedNode = SimplifyOpMul(opNode);
            else if (opNode is OperatorDivNode)
                simplifiedNode = SimplifyOpDiv(opNode);
            else if (opNode is OperatorAddNode || opNode is OperatorSubNode)
                simplifiedNode = SimplifyOpAddSub(opNode);
            else if (opNode is OperatorPowNode)
                simplifiedNode = SimplifyOpPow(opNode);
            return simplifiedNode;
        }

        #endregion

        #region ConcreteNodeSimplify

        private IFuncNode SimplifyOpMul(OperatorNode opNode)
        {
            IFuncNode simplifiedNode = opNode;
            if (ValueOfNodeIs(opNode.LeftChild, 0f) || ValueOfNodeIs(opNode.RightChild, 0f))
                simplifiedNode = new ConstantNode(0);
            else if (ValueOfNodeIs(opNode.LeftChild, 1f))
                simplifiedNode = opNode.RightChild;
            else if (ValueOfNodeIs(opNode.RightChild, 1f))
                simplifiedNode = opNode.LeftChild;
            return simplifiedNode;
        }

        private IFuncNode SimplifyOpDiv(OperatorNode opNode)
        {
            IFuncNode simplifiedNode = opNode;
            if (ValueOfNodeIs(opNode.RightChild, 0f))
                simplifiedNode = new ConstantNode("inf");
            else if (ValueOfNodeIs(opNode.LeftChild, 0f))
                simplifiedNode = new ConstantNode(0f);
            else if (ValueOfNodeIs(opNode.RightChild, 1f))
                simplifiedNode = opNode.LeftChild;
            return simplifiedNode;
        }

        private IFuncNode SimplifyOpAddSub(OperatorNode opNode)
        {
            IFuncNode simplifiedNode = opNode;
            if (ValueOfNodeIs(opNode.RightChild, 0f))
                simplifiedNode = opNode.LeftChild;
            else if (ValueOfNodeIs(opNode.LeftChild, 0f) && !(opNode.RightChild is OperatorNode))
            {
                simplifiedNode = opNode.RightChild;
                if (opNode is OperatorSubNode && !(simplifiedNode is OperatorNode))
                {
                    if (simplifiedNode is VariableNode vNode)
                        vNode.IsPositive = false;
                    else if (simplifiedNode is ConstantNode cNode)
                        cNode.NegateValue();
                }
            }
            return simplifiedNode;
        }

        private IFuncNode SimplifyOpPow(OperatorNode opNode)
        {
            IFuncNode simplifiedNode = opNode;
            if (ValueOfNodeIs(opNode.RightChild, 0f))
                simplifiedNode = new ConstantNode(1f);
            else if (ValueOfNodeIs(opNode.LeftChild, 0f))
                simplifiedNode = new ConstantNode(0f);
            return simplifiedNode;
        }

        private bool ValueOfNodeIs(IFuncNode node, float value)
        {
            if(node is ConstantNode cNode)
                return cNode.GetValue() == value;
            else
                return false;
        }

        #endregion
    }
}