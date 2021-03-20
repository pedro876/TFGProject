using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace FuncSpace
{
    public class Func : IFunc
    {
        public string Name { get; set; }
        public List<string> Variables { get; set; }
        public List<string> Subfunctions { get; set; }
        public string OriginalDeclaration { get; set; }
        public string OriginalDefinition { get; set; }
        public string FinalDeclaration { get; set; }
        public string FinalDefinition { get; set; }
        public IFuncNode RootNode { get; set; }
        public Bytecode BytecodeInfo { get; set; }

        public string ComputeDefinitionString()
        {
            return RootNode.ToString();
        }

        public float Solve(float x, float y, float z)
        {
            return RootNode.Solve(x, y, z);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(FinalDeclaration);
            builder.Append(" = ");
            builder.Append(FinalDefinition);
            return builder.ToString();
        }
    }
}