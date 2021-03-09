using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public static void SetCriterion(Criterion c)
    {
        criterion = c;
        RendererManager.StartRender();
    }

    public static void SetVariable(Variable v)
    {
        variable = v;
        RendererManager.StartRender();
    }

    public static void SetThreshold(float t)
    {
        threshold = t;
        RendererManager.StartRender();
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
