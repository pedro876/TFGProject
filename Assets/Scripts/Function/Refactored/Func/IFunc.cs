using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuncSpace
{
    public interface IFunc
    {
        string GetName();
        void SetName(string name);
        void SetVariables(List<string> variables);
        List<string> GetVariables();
        void SetSubfunctions(List<string> subfunctions);
        List<string> GetSubfunctions();
        string GetOriginalDeclaration();
        string GetFinalDeclaration();
        void SetOriginalDeclaration(string originalDeclaration);
        void SetFinalDeclaration(string originalDeclaration);
        string GetOriginalDefinition();
        string GetFinalDefinition();
        void SetOriginalDefinition(string originalDefintion);
        void SetFinalDefinition(string originalDefinition);
        string ComputeDefinitionString();
        void SetRootNode(IFuncNode root);
    }
}