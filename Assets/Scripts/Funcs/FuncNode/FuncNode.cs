using System.Text;

namespace FuncSpace
{
    public abstract class FuncNode : IFuncNode
    {
        public IFuncNode Parent { get; set; }
        public bool HasParent { get => Parent != null; }
        public bool IsInsideParenthesis { get; set; }

        public bool Equals(IFuncNode otherNode)
        {
            return otherNode.ToString().Equals(ToString());
        }

        public abstract bool NeedsParenthesis();
        public abstract bool NeedsRepresentation();
        public abstract float Solve(float x, float y, float z);

        public abstract void ToStringDeep(StringBuilder builder);
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            ToStringDeep(builder);
            return builder.ToString();
        }
    }
}


