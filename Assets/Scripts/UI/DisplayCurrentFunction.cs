using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayCurrentFunction : MonoBehaviour
{
    TextMeshProUGUI text;
    [SerializeField] float updateTime = 1f;

    // Start is called before the first frame update
    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        text.text = "";
        FunctionPanel.onChanged += UpdateDisplay;
    }

    private void Start()
    {
        StartCoroutine(UpdateDisplayCoroutine());
    }

    IEnumerator UpdateDisplayCoroutine()
    {
        while (true)
        {
            UpdateDisplay();
            yield return new WaitForSeconds(updateTime);
        }
    }

    void UpdateDisplay()
    {
        if (FunctionElement.selectedFunc != null && FunctionElement.selectedFunc.func != null)
        {
            text.text = FunctionElement.selectedFunc.func.ToString();
        }
        else
        {
            text.text = "";
        }
    }

    
}
