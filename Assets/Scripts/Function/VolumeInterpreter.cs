using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VolumeInterpreter : MonoBehaviour
{
    public enum Criterion
    {
        greaterThan,
        lessThan
    }

    public enum Variable
    {
        x,
        y,
        z,
        threshold
    }

    public static float threshold = 0.5f;
    public static Criterion criterion = Criterion.greaterThan;
    public static Variable variable = Variable.z;

    public static event Action onPreChanged;
    public static event Action onChanged;

    private static void OnChanged()
    {
        onPreChanged?.Invoke();
        onChanged?.Invoke();
    }

    public static void SetCriterion(Criterion c)
    {
        criterion = c;
        OnChanged();
        //RendererManager.StartRender();
    }

    public static void SetVariable(Variable v)
    {
        variable = v;
        OnChanged();
        //RendererManager.StartRender();
    }

    public static void SetThreshold(float t)
    {
        threshold = t;
        OnChanged();
        //RendererManager.StartRender();
    }

    public static bool Interpretate(ref Vector3 p, float eval)
    {
        float v = 0f;
        switch (variable)
        {
            case Variable.x: v = p.x; break;
            case Variable.y: v = p.y; break;
            case Variable.z: v = p.z; break;
            case Variable.threshold: v = threshold; break;
        }

        switch (criterion)
        {
            case Criterion.greaterThan: return eval >= v; break;
            case Criterion.lessThan: return eval <= v; break;
        }
        return false;
    }
}
