namespace FuncSpace
{
    public abstract class FuncNode : IFuncNode
    {
        private IFuncNode parent = null;
        bool insideParenthesis = false;

        public IFuncNode GetParent()
        {
            return parent;
        }
        public bool HasParent()
        {
            return parent != null;
        }
        public void SetParent(IFuncNode parent)
        {
            this.parent = parent;
        }
        
        public bool Equals(IFuncNode otherNode)
        {
            return otherNode.ToString().Equals(ToString());
        }

        public bool IsInsideParenthesis()
        {
            return insideParenthesis;
        }
        public void SetParenthesis(bool hasParenthesis)
        {
            insideParenthesis = hasParenthesis;
        }
        public abstract bool NeedsParenthesis();

        public abstract bool NeedsRepresentation();

        public abstract float Solve(float x, float y, float z);
    }
}


