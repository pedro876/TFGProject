using System.Collections.Generic;
using System;

namespace FuncSpace
{
    public static class FuncGeneralInfo
    {
        private readonly static HashSet<string> variables = new HashSet<string>()
        {
            "x","y","z"
        };
        private readonly static List<string> operators = new List<string>()
        {
            "-", "+", "*", "/", "^",
        };
        private readonly static Dictionary<string, int> operatorPriorities = new Dictionary<string, int>()
        {
            { "-", 0 }, { "+", 0 },
            { "*", 1 }, { "/", 1 }, { "^", 1 },
        };

        #region Operators

        public static int MaxOperatorIndex => operators.Count;
        public static int NumOperators => operators.Count;
        public static bool OperatorExists(string op)
        {
            return operators.Contains(op);
        }
        public static int IndexOfOperator(string op)
        {
            return operators.IndexOf(op);
        }
        public static string GetOperatorAtIndex(int index)
        {
            if (index >= 0 && index < NumOperators)
                return operators[index];
            else
                return "";
        }
        public static int GetOperatorPriority(string op)
        {
            if (operatorPriorities.ContainsKey(op))
                return operatorPriorities[op];
            else
                return -1;
        }

        #endregion

        #region Variables

        public static bool VariableExists(string v)
        {
            return variables.Contains(v);
        }

        public static void ForEachVariable(Action<string> action)
        {
            foreach(var variable in variables)
            {
                action(variable);
            }
        }

        #endregion

    }
}