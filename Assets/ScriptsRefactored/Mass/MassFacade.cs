using UnityEngine;
using System;

namespace MassSpace
{
    public class MassFacade : IMassFacade
    {
        private enum CriterionType { Greater, Less, MinDifference }
        private enum VariableType { X, Y, Z, Threshold }

        public event Action onChanged;
        private CriterionType criterion;
        private VariableType variable;
        private float minDifference;
        private float threshold;

        public int Criterion { get => (int)criterion; set { criterion = (CriterionType)(value); onChanged?.Invoke(); } }
        public int Variable { get => (int)variable; set { variable = (VariableType)(value); onChanged?.Invoke(); } }

        public string GetVariableStr()
        {
            return variable.ToString();
        }

        public string GetCriterionStr()
        {
            return criterion.ToString();
        }

        public float MinDifference { get => minDifference; set { minDifference = value; onChanged?.Invoke(); } }
        public float Threshold { get => threshold; set { threshold = value; onChanged?.Invoke(); } }

        public bool IsMass(float x, float y, float z, float eval)
        {
            float v = 0f;
            switch (variable)
            {
                case VariableType.X: v = x; break;
                case VariableType.Y: v = y; break;
                case VariableType.Z: v = z; break;
                case VariableType.Threshold: v = threshold; break;
            }
            switch (criterion)
            {
                case CriterionType.Greater: return eval >= v;
                case CriterionType.Less: return eval <= v;
                case CriterionType.MinDifference: return Mathf.Abs(eval - v) < minDifference;
            }
            return false;
        }
    }
}

