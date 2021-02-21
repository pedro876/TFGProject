using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ViewSizeFixed : MonoBehaviour
{
    [SerializeField] RectTransform functionPanel;
    RectTransform rectTransform;
    AspectRatioFitter fitter;
    /*[SerializeField] */RectTransform parentRectTransform;

    [ExecuteAlways]
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        fitter = GetComponent<AspectRatioFitter>();
        parentRectTransform = transform.parent.GetComponent<RectTransform>();
    }

    [ExecuteAlways]
    void Update()
    {
        if (functionPanel == null || rectTransform == null || fitter == null) return;

        float parentWidth = parentRectTransform ? parentRectTransform.rect.width : Screen.width;
        float parentHeight = parentRectTransform ? parentRectTransform.rect.height : Screen.height;

        float xMax = functionPanel.rect.xMax;
        float width = parentWidth - xMax;

        

        fitter.aspectMode = parentHeight > width ?
            AspectRatioFitter.AspectMode.HeightControlsWidth : 
            AspectRatioFitter.AspectMode.WidthControlsHeight;

        rectTransform.sizeDelta = new Vector2(width, parentHeight);

        rectTransform.anchoredPosition = new Vector2(
            xMax + (width - parentWidth) * 0.5f,
            rectTransform.anchoredPosition.y);
    }
}
