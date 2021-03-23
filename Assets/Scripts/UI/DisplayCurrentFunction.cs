using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayCurrentFunction : MonoBehaviour
{
    private TextMeshProUGUI text;
    private IFuncFacade funcFacade;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();

        funcFacade = ServiceLocator.Instance.GetService<IFuncFacade>();
        funcFacade.onChanged += UpdateDisplay;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        text.text = funcFacade.GetSelectedFunc();
    }
}
