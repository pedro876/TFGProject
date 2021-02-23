using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Renderer : MonoBehaviour
{
    [HideInInspector] public RectTransform texTransform;

    protected Renderer parent;
    protected Renderer[] children;
    protected int width;
    protected int height;
    protected int depth;
    protected int level;
    protected Texture tex;
    protected RawImage image;

    [SerializeField] protected float startX;
    [SerializeField] protected float startY;
    [SerializeField] protected float region;

    int renderCount = 0;

    #region rendering

    public void DeepStop()
    {
        if(level > 0)
            image.enabled = false;
        if(children != null)
        {
            foreach (var c in children) c.DeepStop();
        }
    }

    public void Render()
    {
        renderCount = 0;
        RenderRegion();
        image.enabled = true;
        if(parent != null)
        {
            parent.renderCount++;
            if(parent.renderCount >= 4)
            {
                parent.image.enabled = false;
            }
        }
        if (children != null)
        {
            foreach (var c in children) c.Render();
        }
    }

    protected virtual void RenderRegion()
    {

    }

    #endregion

    #region creation

    public void Init(int level = 0, Renderer parent = null, float startX = 0f, float startY = 1f, float region = 1f)
    {
        this.startX = startX;
        this.startY = startY;
        this.region = region;
        this.parent = parent;
        this.level = level;
        width = RendererManager.setting[level].width;
        height = RendererManager.setting[level].height;
        depth = RendererManager.setting[level].depth;
        CreateTexture();
        image.texture = tex;

        if (RendererManager.DEBUG)
        {
            image.color = Random.ColorHSV();
        }

        if (level+1 < RendererManager.setting.Count)
        {
            CreateChildren();
            AdjustChildrenPositions();
        }

        image.enabled = false;
    }

    protected virtual void CreateTexture()
    {
        GameObject obj = Instantiate(RendererManager.texObjProto, parent != null ? parent.texTransform : RendererManager.functionViewContainer);
        if (parent == null) obj.transform.SetAsFirstSibling();
        obj.name = "tex" + level + "_"+ (parent != null ? ""+parent.texTransform.childCount : "");
        texTransform = obj.GetComponent<RectTransform>();
        image = obj.GetComponent<RawImage>();
    }

    private void CreateChildren()
    {
        children = new Renderer[4];
        for(int i = 0; i < 4; i++)
        {
            var obj = Instantiate(RendererManager.rendererProto, transform);
            obj.name = "renderer" + (level + 1) + "_" + i;
            var r = obj.GetComponent<Renderer>();
            children[i] = r;
            r.Init(level + 1, this, startX+region*positions[i].x, startY+region*positions[i].y, region*0.5f);
        }
    }

    #endregion

    #region position

    private static Vector2[] positions = new Vector2[]
        {
            new Vector2(0f,0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, -0.5f),
            new Vector2(0.5f, -0.5f),
        };

    public void AdjustChildrenPositions()
    {
        if (children == null) return;
        else
        {
            for (int i = 0; i < 4; i++)
            {
                children[i].texTransform.sizeDelta = new Vector2( Mathf.Ceil(texTransform.sizeDelta.x * 0.5f), Mathf.Ceil(texTransform.sizeDelta.y * 0.5f));
                children[i].texTransform.position = new Vector2(
                    Mathf.Floor(texTransform.position.x+texTransform.sizeDelta.x*positions[i].x),
                    Mathf.Floor(texTransform.position.y+texTransform.sizeDelta.y * positions[i].y)
                );
                children[i].AdjustChildrenPositions();
            }
        }
    }

    #endregion
}
