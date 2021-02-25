using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Renderer : MonoBehaviour
{
    [HideInInspector] public RectTransform texTransform;

    private Renderer[] children;
    private int childIndex = 0;

    protected Renderer parent;
    protected RawImage image;
    protected int level;
    protected Color[] memory;
    protected Texture tex;
    protected float startX;
    protected float startY;
    protected float region;
    protected int width;
    protected int height;
    protected int depth;

    protected float[] homogeneities = new float[4];
    protected const int homogeneityPoints = 10;

    private int renderCount = 0;

    [SerializeField] protected bool done = false;
    [SerializeField] protected bool rendering = false;

    #region Homogeneity

    protected float GetHomogeneity()
    {
        if (parent == null) return 0f;
        else return parent.homogeneities[childIndex];
    }

    protected float GetDisparity()
    {
        return 1f - GetHomogeneity();
    }

    protected float GetImportance()
    {
        return GetDisparity() + RendererManager.maxLevel - level;
    }

    protected virtual void CalculateHomogeneity()
    {
        /*if(level == RendererManager.setting.Count)
        {
            for (int i = 0; i < homogeneities.Length; i++) homogeneities[i] = 1f;
            return;
        }*/

        List<float[]> randomDepths = new List<float[]>()
        {
            GetRandomDepths(0,0,width/2,height/2),
            GetRandomDepths(width/2,0,width,height/2),
            GetRandomDepths(0,height/2,width/2,height),
            GetRandomDepths(width/2,height/2,width,height),
        };
        for (int i = 0; i < homogeneities.Length; i++) homogeneities[i] = 0f;

        float totalDistances = 0f;
        for (int i = 0; i < homogeneityPoints; i++)
        {
            for (int j = 0; j < homogeneityPoints; j++)
            {
                homogeneities[0] += Mathf.Abs(randomDepths[0][i] - randomDepths[0][j]);
                homogeneities[1] += Mathf.Abs(randomDepths[1][i] - randomDepths[1][j]);
                homogeneities[2] += Mathf.Abs(randomDepths[2][i] - randomDepths[2][j]);
                homogeneities[3] += Mathf.Abs(randomDepths[3][i] - randomDepths[3][j]);
                totalDistances++;
            }
        }

        for (int i = 0; i < homogeneities.Length; i++) homogeneities[i] = 1f-homogeneities[i]/totalDistances;
    }

    protected abstract float[] GetRandomDepths(int minX, int minY, int maxX, int maxY);

    #endregion

    #region rendering

    public virtual void DeepStop()
    {
        image.enabled = level == 0;
        /*if(level > 0)
            image.enabled = false;*/
        if(children != null)
        {
            foreach (var c in children) c.DeepStop();
        }
    }

    private void Update()
    {
        if(rendering && done)
        {
            MemoryToTex();
            rendering = false;
            RenderChildren();
        }
    }

    protected virtual void MemoryToTex()
    {
        
    }

    public virtual void Render()
    {
        done = false;
        rendering = true;
        renderCount = 0;
        if (level > 0) image.enabled = false;
        if (FunctionElement.selectedFunc == null || FunctionElement.selectedFunc.func == null)
        {
            done = true;
            rendering = false;
        }
    }

    protected void RenderChildren()
    {
        CalculateHomogeneity();
        if (parent != null)
        {
            parent.renderCount++;
            if (parent.renderCount >= 4)
            {
                parent.image.enabled = false;
            }
        }
        if (children != null)
        {
            foreach (var c in children) c.Render();
        }
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
        memory = new Color[width*height];
        CreateTexture();
        image.texture = tex;

        if (RendererManager.DEBUG)
        {
            image.color = Random.ColorHSV();
        }

        if (level+1 < RendererManager.maxLevel)
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
            var obj = Instantiate(RendererManager.cpuRendererProto, transform);
            obj.name = "renderer" + (level + 1) + "_" + i;
            var r = obj.GetComponent<Renderer>();
            children[i] = r;
            r.childIndex = i;
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
                children[i].texTransform.sizeDelta = new Vector2(texTransform.sizeDelta.x * 0.5f, texTransform.sizeDelta.y * 0.5f);
                children[i].texTransform.position = new Vector2(
                    texTransform.position.x+texTransform.sizeDelta.x*positions[i].x,
                    texTransform.position.y+texTransform.sizeDelta.y * positions[i].y
                );
                children[i].AdjustChildrenPositions();
            }
        }
    }

    #endregion
}
