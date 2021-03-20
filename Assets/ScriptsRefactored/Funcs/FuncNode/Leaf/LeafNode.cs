using System.Collections;
using UnityEngine;

namespace FuncSpace
{
    public abstract class LeafNode : FuncNode
    {
        public abstract override float Solve(float x, float y, float z);
    }
}