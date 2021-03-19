using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuncSpace
{
    public abstract class NonLeafNode : FuncNode
    {
        private List<IFuncNode> children;
        private float[] childrenResults;

        public NonLeafNode(List<IFuncNode> children)
        {
            this.children = children;
            childrenResults = new float[this.children.Count];
        }

        public IFuncNode GetChild(int childIndex)
        {
            return children[0];
        }

        public List<IFuncNode> GetChildren()
        {
            return children;
        }

        public void AddChild(IFuncNode child)
        {
            children.Add(child);
        }

        public override float Solve(float x, float y, float z)
        {
            SolveChildren(x, y, z);
            float result = SolveSelf(childrenResults);
            return result;
        }

        protected void SolveChildren(float x, float y, float z)
        {
            for(int i = 0; i < children.Count; i++)
            {
                childrenResults[i] = children[i].Solve(x, y, z);
            }
        }

        protected abstract float SolveSelf(float[] values);

        public override bool NeedsRepresentation()
        {
            return true;
        }
    }
}