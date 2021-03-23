using System;

public interface IMassFacade
{
    float MinDifference { get; set; }
    float Threshold { get; set; }
    int Criterion { get; set; }
    int Variable { get; set; }
    string GetVariableStr();
    string GetCriterionStr();
    event Action onChanged;
    bool IsMass(float x, float y, float z, float eval);
    int GreaterCriterion { get; }
    int LessCriterion { get; }
    int MinDifferenceCriterion { get; }
    int VariableX { get; }
    int VariableY { get; }
    int VariableZ { get; }
    int VariableThreshold { get; }
}