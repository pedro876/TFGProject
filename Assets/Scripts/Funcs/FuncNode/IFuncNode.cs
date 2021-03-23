using System.Text;

namespace FuncSpace
{
    public interface IFuncNode
    {
        IFuncNode Parent { get; set; }
        bool HasParent { get; }
        bool IsInsideParenthesis { get; set; }
        float Solve(float x, float y, float z);
        bool Equals(IFuncNode otherNode);
        bool NeedsParenthesis();
        bool NeedsRepresentation();
        void ToStringDeep(StringBuilder builder);
    }
}


