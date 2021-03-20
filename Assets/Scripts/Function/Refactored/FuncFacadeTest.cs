using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FuncFacadeTest : MonoBehaviour
{
    KeyValuePair<string, Func<bool>>[] tests;

    void Start()
    {
        tests = new KeyValuePair<string, Func<bool>>[]
        {
            new KeyValuePair<string, Func<bool>>("TestInstantSolve", TestInstantSolve),
            new KeyValuePair<string, Func<bool>>("TestFacadeCreation", TestFacadeCreation),
            new KeyValuePair<string, Func<bool>>("TestCreateFunc1", TestCreateFunc1),
            new KeyValuePair<string, Func<bool>>("TestCreateFunc2", TestCreateFunc2),
            new KeyValuePair<string, Func<bool>>("TestCreateFunc3", TestCreateFunc3),
            new KeyValuePair<string, Func<bool>>("TestCreateFunc4", TestCreateFunc4),
            new KeyValuePair<string, Func<bool>>("TestCreateFunc5", TestCreateFunc5),
            new KeyValuePair<string, Func<bool>>("TestCreateFunc6", TestCreateFunc6),
            new KeyValuePair<string, Func<bool>>("TestCreateFunc7", TestCreateFunc7),
            new KeyValuePair<string, Func<bool>>("TestSolve1", TestSolve1),
            new KeyValuePair<string, Func<bool>>("TestNonReference", TestNonReference),
            new KeyValuePair<string, Func<bool>>("TestCrossReference1", TestCrossReference1),
            new KeyValuePair<string, Func<bool>>("TestCrossReference2", TestCrossReference2),
            new KeyValuePair<string, Func<bool>>("TestCrossReference2", TestCrossReference2),
            new KeyValuePair<string, Func<bool>>("TestSolveCrossReference1", TestSolveCrossReference1),
            new KeyValuePair<string, Func<bool>>("TestSolveCrossReference2", TestSolveCrossReference2),
        };

        foreach(var test in tests)
        {
            Debug.Log("Testing " + test.Key);
            bool success = test.Value.Invoke();
            if (success) Debug.Log("SUCCESS");
            else Debug.LogError("FAILURE");
        }
    }

    bool TestInstantSolve()
    {
        FuncFacade facade = FuncFacade.Instance;
        facade.Reset();
        string result = facade.Solve(1, 0, 0).ToString();
        string expectedResult = "1";
        Debug.Log("expected:\t" + expectedResult);
        Debug.Log("got:\t\t" + result);
        return expectedResult.Equals(result);
    }

    bool TestFacadeCreation()
    {
        FuncFacade facade = FuncFacade.Instance;
        facade.Reset();
        string expectedResult = "dummy(x) = x";

        string result = facade.SelectedFunc;

        Debug.Log("expected:\t" + expectedResult);
        Debug.Log("got:\t\t" + result);

        return expectedResult.Equals(result);
    }

    bool TestCreateFunc1()
    {
        FuncFacade facade = FuncFacade.Instance;
        facade.Reset();
        string expectedResult = "f(x) = x^2";
        facade.CreateFunc(expectedResult);
        facade.SelectFunc("f");
        string result = facade.SelectedFunc;

        Debug.Log("expected:\t" + expectedResult);
        Debug.Log("got:\t\t" + result);

        return expectedResult.Equals(result);
    }

    bool TestCreateFunc2()
    {
        FuncFacade facade = FuncFacade.Instance;
        facade.Reset();
        string expectedResult = "f(x) = x^2*-(5*x)";
        facade.CreateFunc(expectedResult);
        facade.SelectFunc("f");
        string result = facade.SelectedFunc;

        Debug.Log("expected:\t" + expectedResult);
        Debug.Log("got:\t\t" + result);

        return expectedResult.Equals(result);
    }

    bool TestCreateFunc3()
    {
        FuncFacade facade = FuncFacade.Instance;
        facade.Reset();
        string expectedResult = "g(x) = cos(x)+cos(y)";
        facade.CreateFunc(expectedResult);
        facade.SelectFunc("g");
        string result = facade.SelectedFunc;

        expectedResult = "g(x,y) = cos(x)+cos(y)";
        Debug.Log("expected:\t" + expectedResult);
        Debug.Log("got:\t\t" + result);

        return expectedResult.Equals(result);
    }
    bool TestCreateFunc4()
    {
        FuncFacade facade = FuncFacade.Instance;
        facade.Reset();
        string expectedResult = "g(x) = x-+y";
        facade.CreateFunc(expectedResult);
        facade.SelectFunc("g");
        string result = facade.SelectedFunc;

        expectedResult = "g(x,y) = x-y";
        Debug.Log("expected:\t" + expectedResult);
        Debug.Log("got:\t\t" + result);

        return expectedResult.Equals(result);
    }

    bool TestCreateFunc5()
    {
        FuncFacade facade = FuncFacade.Instance;
        facade.Reset();
        string expectedResult = "g(x) = x+-y";
        facade.CreateFunc(expectedResult);
        facade.SelectFunc("g");
        string result = facade.SelectedFunc;

        expectedResult = "g(x,y) = x-y";
        Debug.Log("expected:\t" + expectedResult);
        Debug.Log("got:\t\t" + result);

        return expectedResult.Equals(result);
    }

    bool TestCreateFunc6()
    {
        FuncFacade facade = FuncFacade.Instance;
        facade.Reset();
        string expectedResult = "g(x) = -x-y";
        facade.CreateFunc(expectedResult);
        facade.SelectFunc("g");
        string result = facade.SelectedFunc;

        expectedResult = "g(x,y) = -(x+y)";
        Debug.Log("expected:\t" + expectedResult);
        Debug.Log("got:\t\t" + result);

        return expectedResult.Equals(result);
    }

    bool TestCreateFunc7()
    {
        FuncFacade facade = FuncFacade.Instance;
        facade.Reset();
        string expectedResult = "g(x,y) = sin((x+y)*20)*0.5";
        facade.CreateFunc(expectedResult);
        facade.SelectFunc("g");
        string result = facade.SelectedFunc;

        expectedResult = "g(x,y) = sin((x+y)*20)*0,5";
        Debug.Log("expected:\t" + expectedResult);
        Debug.Log("got:\t\t" + result);

        return expectedResult.Equals(result);
    }


    bool TestSolve1()
    {
        FuncFacade facade = FuncFacade.Instance;
        facade.Reset();
        string func = "g(x) = cos(x)+cos(y)";
        facade.CreateFunc(func);
        facade.SelectFunc("g");
        float x = 3.9f;
        float y = 2f;
        string expectedResult = (Mathf.Cos(x)+Mathf.Cos(y)).ToString();
        string result = facade.Solve(x, y, 0f).ToString();

        Debug.Log("expected:\t" + expectedResult);
        Debug.Log("got:\t\t" + result);
        return expectedResult.Equals(result);
    }

    bool TestNonReference()
    {
        FuncFacade facade = FuncFacade.Instance;
        facade.Reset();
        string func = "f(x) = g(x)";
        facade.CreateFunc(func);
        facade.SelectFunc("f");
        string expectedResult = "f = 1";
        string result = facade.SelectedFunc;

        Debug.Log("expected:\t" + expectedResult);
        Debug.Log("got:\t\t" + result);
        return expectedResult.Equals(result);
    }

    bool TestCrossReference1()
    {
        FuncFacade facade = FuncFacade.Instance;
        facade.Reset();
        string func1 = "f(x) = g(x)";
        string func2 = "g(x) = x";
        facade.CreateFunc(func1);
        facade.CreateFunc(func2);
        facade.SelectFunc("f");
        string expectedResult = "f(x) = g(x)";
        string result = facade.SelectedFunc;

        Debug.Log("expected:\t" + expectedResult);
        Debug.Log("got:\t\t" + result);
        return expectedResult.Equals(result);
    }

    bool TestCrossReference2()
    {
        FuncFacade facade = FuncFacade.Instance;
        facade.Reset();
        string func1 = "f(x) = g(x)";
        string func2 = "g(x) = f(x)";
        facade.CreateFunc(func1);
        facade.CreateFunc(func2);
        facade.SelectFunc("g");
        string expectedResult = func2;
        string result = facade.SelectedFunc;

        Debug.Log("expected:\t" + expectedResult);
        Debug.Log("got:\t\t" + result);
        return expectedResult.Equals(result);
    }

    bool TestSolveCrossReference1()
    {
        FuncFacade facade = FuncFacade.Instance;
        facade.Reset();
        string func1 = "f(x) = x";
        string func2 = "g(x) = f(x)";
        facade.CreateFunc(func1);
        facade.CreateFunc(func2);
        facade.SelectFunc("g");
        string expectedResult = "2";
        string result = facade.Solve(2,0,0).ToString();

        Debug.Log("expected:\t" + expectedResult);
        Debug.Log("got:\t\t" + result);
        return expectedResult.Equals(result);
    }

    bool TestSolveCrossReference2()
    {
        FuncFacade facade = FuncFacade.Instance;
        facade.Reset();
        string func1 = "f(x) = g(x)";
        string func2 = "g(x) = f(x)";
        facade.CreateFunc(func1);
        facade.CreateFunc(func2);
        facade.SelectFunc("g");
        string expectedResult = "1";
        string result = facade.Solve(2, 0, 0).ToString();

        Debug.Log("expected:\t" + expectedResult);
        Debug.Log("got:\t\t" + result);
        return expectedResult.Equals(result);
    }
}
