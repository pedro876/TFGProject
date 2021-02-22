using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionManager
{
    private static FunctionReader reader = new FunctionReader();

    public static Dictionary<string, Function> functions = new Dictionary<string, Function>()
    {

    };

    private static Dictionary<string, float> variables = new Dictionary<string, float>()
    {
        { "pi", Mathf.PI },
        { "e", Mathf.Exp(1) },
        { "inf", Mathf.Infinity * 0.8f },
    };

    public static bool TryGetVariable(string key, out float result)
    {
        result = 0f;
        return variables.TryGetValue(key, out result);
    }

    public static bool HasVariable(string key)
    {
        return variables.ContainsKey(key);
    }

    public static Function AddFunction(Function func)
    {
        functions[func.name] = func;
        return func;
    }
    public static Function AddFunction(string func)
    {
        Function f = reader.ProcessFunc(func);
        if(f != null)
        {
            functions[func] = f;
        }
        return f;
    }

    public static void RemoveFunction(string func)
    {
        functions.Remove(func);
    }
}
