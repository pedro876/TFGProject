using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VolumeInterpreter : MonoBehaviour
{
    public enum CriterionType { GreaterThan, LessThan }
    public enum VariableType { X, Y, Z, Threshold }

    private static float threshold = 0.5f;
    private static CriterionType criterion = CriterionType.GreaterThan;
    private static VariableType variable = VariableType.Z;

    public static float Threshold
    { 
        get { return threshold; }
        set 
        {
            threshold = value;
            OnChanged();
        }
    }
    
    public static CriterionType Criterion
    {
        get { return criterion; }
        set
        {
            criterion = value;
            OnChanged();
        }
    }
    
    public static VariableType Variable
    {
        get { return variable; }
        set
        {
            variable = value;
            OnChanged();
        }
    }

    public static event Action onPreChanged;
    public static event Action onChanged;

    private static void OnChanged()
    {
        onPreChanged?.Invoke();
        onChanged?.Invoke();
    }

    public static bool Interpretate(ref Vector3 p, float eval)
    {
        float v = 0f;
        switch (variable)
        {
            case VariableType.X: v = p.x; break;
            case VariableType.Y: v = p.y; break;
            case VariableType.Z: v = p.z; break;
            case VariableType.Threshold: v = threshold; break;
        }
        switch (criterion)
        {
            case CriterionType.GreaterThan: return eval >= v;
            case CriterionType.LessThan: return eval <= v;
        }
        return false;
    }
}
