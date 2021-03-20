using System.Collections.Generic;

namespace FuncSpace
{
    public interface IFunc
    {
        string Name { get; set; }
        string OriginalDeclaration { get; set; }
        string OriginalDefinition { get; set; }
        string FinalDeclaration { get; set; }
        string FinalDefinition { get; set; }

        List<string> Variables { get; set; }
        List<string> Subfunctions { get; set; }

        Bytecode BytecodeInfo { get; set; }

        IFuncNode RootNode { get; set; }
        string ComputeDefinitionString();
        float Solve(float x, float y, float z);

    }
}