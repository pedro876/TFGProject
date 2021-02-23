using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RendererManager : MonoBehaviour
{
    Renderer rootRenderer;
    [SerializeField] GameObject localTexObjProto;
    [SerializeField] GameObject localRendererProto;
    public static GameObject rendererProto;
    public static GameObject texObjProto;
    public static RectTransform spaceView;
    public static RectTransform functionViewContainer;

    public const bool DEBUG = false;

    private void Start()
    {
        texObjProto = localTexObjProto;
        rendererProto = localRendererProto;
        spaceView = FindObjectOfType<ViewController>().GetComponent<RectTransform>();
        functionViewContainer = GameObject.FindGameObjectWithTag("FuncContainer").GetComponent<RectTransform>();
        rootRenderer = GetComponentInChildren<Renderer>();

        rootRenderer.Init(0);
        AdjustPositions();
        FunctionPanel.onChanged += StartRender;
        ViewController.onChanged += StartRender;
        StartRender();
    }

    

    private void LateUpdate()
    {
        AdjustPositions();
    }

    private void AdjustPositions()
    {
        rootRenderer.texTransform.position = spaceView.position+new Vector3(-spaceView.sizeDelta.x*0.5f, spaceView.sizeDelta.y*0.5f, 0f);
        rootRenderer.texTransform.sizeDelta = spaceView.sizeDelta;
        rootRenderer.AdjustChildrenPositions();
    }

    #region rendering

    private void StartRender()
    {
        //Debug.Log("Start render");
        StopCoroutine(RenderCoroutine());

        rootRenderer.DeepStop();

        StartCoroutine(RenderCoroutine());
    }

    private void Render()
    {
        rootRenderer.Render();
    }

    IEnumerator RenderCoroutine()
    {
        yield return null;
        Render();
    }



    #endregion

    #region Quality

    public class RendererQuality
    {
        public readonly int width;
        public readonly int height;
        public readonly int depth;

        public RendererQuality(int size, int depth)
        {
            this.width = size;
            this.height = size;
            this.depth = depth;
        }
    }

    public static List<RendererQuality> setting = new List<RendererQuality>()
    {
        new RendererQuality(32,32),
        new RendererQuality(32,64),
        new RendererQuality(32,128),
        new RendererQuality(32,128),
        /*new RendererQuality(64,128),
        new RendererQuality(128,256),*/
    };

    #endregion
}
