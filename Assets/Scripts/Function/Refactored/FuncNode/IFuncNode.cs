namespace FuncSpace
{
    public interface IFuncNode
    {
        IFuncNode GetParent();
        bool HasParent();
        void SetParent(IFuncNode parent);
        float Solve(float x, float y, float z);
        //int CalculateBytecode();
        bool Equals(IFuncNode otherNode);

        bool IsInsideParenthesis();
        void SetParenthesis(bool hasParenthesis);
        bool NeedsParenthesis();
        bool NeedsRepresentation();
    }
}


