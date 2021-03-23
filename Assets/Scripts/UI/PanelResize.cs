using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PanelResize : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] CanvasScaler canvasScaler;
    [SerializeField] RectTransform panel;
    [SerializeField] float sizeMultiplierOnHover = 2f;

    private float defaultSize;
    private Texture2D widthCursorTex;
    private bool drag = false;
    private RectTransform parent;
    private RectTransform selfTransform;

    void Start()
    {
        widthCursorTex = Resources.Load("Cursors/widthCursor") as Texture2D;
        parent = panel.parent.GetComponent<RectTransform>();
        selfTransform = GetComponent<RectTransform>();
        defaultSize = selfTransform.sizeDelta.x;
    }

    #region Resize

    void Update()
    {
        ClampMaxSize();

        if (drag)
        {
            Drag();
        }
    }

    private void ClampMaxSize()
    {
        if (panel.sizeDelta.x > parent.rect.width - defaultSize)
            panel.sizeDelta = new Vector2(parent.rect.width - defaultSize, panel.sizeDelta.y);
    }

    private void Drag()
    {
        float x = Input.mousePosition.x / canvasScaler.scaleFactor;
        if (panel != null)
        {
            float size = x - panel.rect.xMin;
            size = Mathf.Clamp(size, 0f, parent.rect.width - defaultSize);
            panel.sizeDelta = new Vector2(size, panel.sizeDelta.y);
        }
    }

    #endregion

    #region PointerEvents

    public void OnPointerEnter(PointerEventData eventData)
    {
        Texture2D tex = widthCursorTex;
        Cursor.SetCursor(tex, Vector2.zero, CursorMode.Auto);
        selfTransform.sizeDelta = new Vector2(defaultSize * sizeMultiplierOnHover, selfTransform.sizeDelta.y);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!drag)
        {
            selfTransform.sizeDelta = new Vector2(defaultSize, selfTransform.sizeDelta.y);
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        drag = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        drag = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        selfTransform.sizeDelta = new Vector2(defaultSize, selfTransform.sizeDelta.y);
    }

    #endregion
}
