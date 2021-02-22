using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FunctionPanel : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;
    [SerializeField] TextMeshProUGUI displayText;

    void Start()
    {
        StartCoroutine(UpdateFunction());
    }

    IEnumerator UpdateFunction()
    {
        while (true)
        {
            if(inputField != null && displayText != null)
            {
                if (inputField.text.Contains("="))
                {
                    Function f = FunctionManager.AddFunction(inputField.text);

                    //Fix declaration and cursor location
                    string[] parts = inputField.text.Split('=');
                    int displacement = f.declaration.Length + 1 - parts[0].Length;
                    inputField.text = f.declaration + " =" + parts[1];
                    if (displacement > 0)
                    {
                        inputField.selectionAnchorPosition += displacement;
                        inputField.selectionFocusPosition += displacement;
                    }

                    //Display processed function
                    displayText.text = f.ToString();
                }
            }

            yield return new WaitForSeconds(0.2f);
        }
    }
}
