using System.Text;

namespace FuncSpace
{
    public class VariableNode : LeafNode
    {
        private string variable;
        private bool positive;
        public bool IsPositive { get; set; }
        public string Variable { get => variable; }
        public VariableNode(string variable)
        {
            this.variable = variable;
            positive = true;
        }

        public override bool NeedsRepresentation()
        {
            return true;
        }

        public override bool NeedsParenthesis()
        {
            if (!HasParent) return false;
            else return !positive;
        }

        public override void ToStringDeep(StringBuilder builder)
        {
            bool needsParenthesis = NeedsParenthesis();
            if (needsParenthesis) builder.Append('(');
            if (!positive) builder.Append('-');
            builder.Append(variable);
            if (needsParenthesis) builder.Append(')');
        }
    }
}