using UnityEngine;
using System;

namespace MassSpace
{
    public class MassFacade : MonoBehaviour, IMassFacade
    {
        private enum CriterionType { Greater, Less, MinDifference }
        private enum VariableType { X, Y, Z, Threshold }

        public event Action onChanged;
        public int Criterion { get => (int)criterion; set { criterion = (CriterionType)(value); onChanged?.Invoke(); } }
        public int Variable { get => (int)variable; set { variable = (VariableType)(value); onChanged?.Invoke(); } }

        public int GreaterCriterion => (int)CriterionType.Greater;
        public int LessCriterion => (int)CriterionType.Less;
        public int MinDifferenceCriterion => (int)CriterionType.MinDifference;
        public int VariableX => (int)VariableType.X;
        public int VariableY => (int)VariableType.Y;
        public int VariableZ => (int)VariableType.Z;
        public int VariableThreshold => (int)VariableType.Threshold;
        public bool AutoMode { get; set; }

        private CriterionType criterion;
        private VariableType variable;
        private float minDifference;
        private float threshold;

        private IFuncFacade funcFacade;

        private void Awake()
        {
            threshold = 0.5f;
            minDifference = 0.4f;
            Variable = VariableZ;
            AutoMode = true;
        }

        private void Start()
        {
            funcFacade = ServiceLocator.Instance.GetService<IFuncFacade>();
            funcFacade.onChanged += DecideModeAuto;
        }

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

        public bool IsMass(ref Vector3 pos, float eval)
        {
            float v = 0f;
            switch (variable)
            {
                case VariableType.X: v = pos.x; break;
                case VariableType.Y: v = pos.y; break;
                case VariableType.Z: v = pos.z; break;
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

        private void DecideModeAuto()
        {
            if (AutoMode)
            {
                bool hasX = funcFacade.SelectedFuncUsesVariable("x");
                bool hasY = funcFacade.SelectedFuncUsesVariable("y");
                bool hasZ = funcFacade.SelectedFuncUsesVariable("z");

                Criterion = GreaterCriterion;

                if (hasX && hasY && hasZ)
                    Variable = VariableThreshold;
                else if (hasX && hasY)
                    Variable = VariableZ;
                else if (hasY && hasZ)
                    Variable = VariableX;
                else if (hasX && hasZ)
                    Variable = VariableY;
                else if (hasX)
                    Variable = VariableZ;
                else if (hasY)
                    Variable = VariableZ;
                else if (hasZ)
                    Variable = VariableX;
            }
        }
    }
}

