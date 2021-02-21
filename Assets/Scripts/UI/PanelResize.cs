using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PanelResize : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{

    //Button btn;
    const string resizeCursor = "resizeCursor";
    Texture2D resizeCursorTex;

    bool drag = false;

    [SerializeField] RectTransform panel;

    void Start()
    {
        //btn = GetComponent<Button>();
        resizeCursorTex = Resources.Load("Cursors/resizeCursor") as Texture2D;
    }
    
    void Update()
    {
        if (drag)
        {
            float x = Input.mousePosition.x;
            if(panel != null)
            {
                float size = x - panel.rect.xMin;
                panel.sizeDelta = new Vector2(size, panel.sizeDelta.y);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Cursor.SetCursor(resizeCursorTex, Vector2.zero, CursorMode.Auto);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(!drag)
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        drag = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        drag = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
