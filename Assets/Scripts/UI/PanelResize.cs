using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PanelResize : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] float sizeMultiplierOnHover = 2f;
    float defaultSize;
    [SerializeField] bool resizeWidth = true;
    //[SerializeField] bool resizeHeight = false;
    //Button btn;
    //const string resizeCursor = "resizeCursor";
    Texture2D widthCursorTex;
    //Texture2D heightCursorTex;

    bool drag = false;

    [SerializeField] RectTransform panel;
    RectTransform parent;
    RectTransform selfTransform;

    void Start()
    {
        widthCursorTex = Resources.Load("Cursors/widthCursor") as Texture2D;
        //heightCursorTex = Resources.Load("Cursors/heightCursor") as Texture2D;
        parent = panel.parent.GetComponent<RectTransform>();
        selfTransform = GetComponent<RectTransform>();
        defaultSize = selfTransform.sizeDelta.x;
    }
    
    void Update()
    {
        if (panel.sizeDelta.x > parent.rect.width - defaultSize) panel.sizeDelta = new Vector2(parent.rect.width - defaultSize, panel.sizeDelta.y);

        if (drag)
        {
            if (resizeWidth)
            {
                float x = Input.mousePosition.x;
                if (panel != null)
                {
                    float size = x - panel.rect.xMin;
                    size = Mathf.Clamp(size, 0f, parent.rect.width-defaultSize);
                    panel.sizeDelta = new Vector2(size, panel.sizeDelta.y);
                }
            }
            //if (resizeHeight)
            //{
            //    float y = /*Screen.height - */Input.mousePosition.y;
            //    if (panel != null)
            //    {
            //        //float size = panel.rect.yMin+y;
            //        float size = y - panel.position.y;
            //        size = Mathf.Abs(size);
            //        //Debug.Log(size);
            //        panel.sizeDelta = new Vector2(panel.sizeDelta.x,size);
            //    }
            //}
            
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Texture2D tex = null;
        if (resizeWidth) tex = widthCursorTex;
        //if (resizeHeight) tex = heightCursorTex;
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
}
