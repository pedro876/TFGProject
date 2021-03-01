using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class DepthCameraSize : MonoBehaviour
{
    RectTransform parent;
    Camera cam;

    [ExecuteAlways]
    void Start()
    {
        cam = GetComponent<Camera>();
        parent = transform.parent.GetComponent<RectTransform>();
    }

    [ExecuteAlways]
    void Update()
    {
        cam.orthographicSize = parent.rect.width / 2f;
        cam.Render();
    }
}
