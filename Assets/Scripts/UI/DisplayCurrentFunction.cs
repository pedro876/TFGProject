using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayCurrentFunction : MonoBehaviour
{
    TextMeshProUGUI text;
    [SerializeField] float updateTime = 1f;

    bool mustUpdateDisplay = true;

    // Start is called before the first frame update
    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        text.text = "";
        FunctionPanel.onChanged += ()=>mustUpdateDisplay = true;
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
        if (mustUpdateDisplay)
        {
            mustUpdateDisplay = !FunctionElement.hasValidFunc;
            text.text = FunctionElement.hasValidFunc ? FunctionElement.selectedFunc.func.ToString() : "";
        }
    }

    
}
