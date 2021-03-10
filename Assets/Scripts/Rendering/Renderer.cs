using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public abstract class Renderer : MonoBehaviour
{
    [HideInInspector] public RectTransform texTransform;

    [HideInInspector] public Renderer[] children;
    private int childIndex = 0;

    protected Renderer parent;
    protected RawImage image;
    protected int level;
    protected Color[] depthMemory;
    protected Color[] normalMemory;
    protected Texture depthTex;
    protected Texture normalTex;
    protected float startX;
    protected float startY;
    protected float region;
    protected int width;
    protected int height;
    protected int depth;
    //protected float normalExplorationRadius;
    public static int explorationSamples = 10;
    public static float depthExplorationMultiplier = 1.05f;
    public static float normalExplorationMultiplier = 1f;
    public static float normalPlaneMultiplier = 0.2f;

    protected float[] homogeneities = new float[4];
    protected const int homogeneityPoints = 10;

    private int renderCount = 0;

    protected bool done = false;
    protected bool rendering = false;
    protected bool queued = false;
    private bool renderingChildren = false;
    protected bool texApplied = false;
    public bool deepFinished = false;

    public static event Action onTexApplied;

    #region State

    public bool IsQueued { get => queued; }
    public bool IsRendering { get => rendering && !queued; }
    public bool IsDone { get => done; }
    public bool IsRenderingChildren { get => renderingChildren; }
    public bool IsTextureApplied { get => texApplied; }

    #endregion

    #region Homogeneity

    public float GetHomogeneity()
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

        for (int i = 0; i < homogeneities.Length; i++) homogeneities[i] = 1f-Mathf.Clamp(homogeneities[i]/totalDistances,0f,1f);
    }

    protected abstract float[] GetRandomDepths(int minX, int minY, int maxX, int maxY);

    #endregion

    #region rendering

    /*private static readonly Vector3[] dirsExplored = new Vector3[]
    {
        new Vector3(1,0,0),
        new Vector3(-1,0,0),
        new Vector3(0,1,0),
        new Vector3(0,-1,0),
        new Vector3(0,0,1),
        new Vector3(0,0,-1),
    };*/

    //protected readonly Vector3[] dirsExplored = new Vector3[6];

    //private void UpdateNormalExplorationRadius()
    //{
    //    /*dirsExplored[0] =  ViewController.camTransform.forward;
    //    dirsExplored[1] = -ViewController.camTransform.forward;
    //    dirsExplored[2] =  ViewController.camTransform.right;
    //    dirsExplored[3] = -ViewController.camTransform.right;
    //    dirsExplored[4] =  ViewController.camTransform.up;
    //    dirsExplored[5] = -ViewController.camTransform.up;*/
    //
    //    dirsExplored[0] =  Vector3.up;
    //    dirsExplored[1] = -Vector3.up;
    //    dirsExplored[2] =  Vector3.right;
    //    dirsExplored[3] = -Vector3.right;
    //    dirsExplored[4] =  Vector3.forward;
    //    dirsExplored[5] = -Vector3.forward;
    //
    //    normalExplorationRadius = (Vector3.Distance(ViewController.nearTopLeft, ViewController.farTopLeft) / depth) * explorationRadiusMultiplier;
    //
    //
    //}

    public virtual void DeepStop()
    {
        image.enabled = level == 0;
        done = false;
        renderingChildren = false;
        queued = false;
        rendering = false;
        texApplied = false;
        deepFinished = false;
        if (children != null)
        {
            foreach (var c in children) c.DeepStop();
        }
    }

    private void LateUpdate()
    {
        if(rendering && done && level > 0)
        {
            MemoryToTex();

            rendering = false;
            RenderChildren();
        }
    }

    protected virtual void MemoryToTex()
    {
        image.enabled = true;
        texApplied = true;
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
        onTexApplied?.Invoke();
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
            renderingChildren = true;
            foreach (var c in children) c.Render();
        } else
        {
            RenderFinishedSignal();
        }
    }

    protected void RenderFinishedSignal()
    {
        deepFinished = true;
        if(children != null)
        {
            foreach (Renderer r in children)
                if (!r.deepFinished)
                    deepFinished = false;
        }
        
        if (deepFinished && level > 0) parent.RenderFinishedSignal();
    }

    #endregion

    #region creation

    public void Init(int level = 0, Renderer parent = null, float startX = 0f, float startY = 1f, float region = 1f)
    {
        children = null;
        this.startX = startX;
        this.startY = startY;
        this.region = region;
        this.parent = parent;
        this.level = level;
        width = RendererManager.setting[level].width;
        height = RendererManager.setting[level].height;
        depth = RendererManager.setting[level].depth;
        depthMemory = new Color[width*height];
        normalMemory = new Color[width * height];
        CreateTextures();

        image.texture = normalTex;

        if (RendererManager.DEBUG)
        {
            image.color = UnityEngine.Random.ColorHSV();
        }

        if (level+1 < RendererManager.maxLevel)
        {
            CreateChildren();
            AdjustChildrenPositions();
        }

        image.enabled = false;
    }

    protected virtual void CreateTextures()
    {
        GameObject obj = Instantiate(RendererManager.texObjProto, parent != null ? parent.texTransform : RendererManager.quadContainer);
        if (parent == null) obj.transform.SetAsFirstSibling();
        obj.name = "tex" + level + "_"+ (parent != null ? ""+parent.texTransform.childCount : "");
        texTransform = obj.GetComponent<RectTransform>();
        if (level == 0)
        {
            texTransform.position = RendererManager.quadContainer.position;
            texTransform.sizeDelta = RendererManager.quadContainer.sizeDelta;
        }

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
                children[i].texTransform.position = new Vector3(
                    texTransform.position.x+texTransform.sizeDelta.x*positions[i].x,
                    texTransform.position.y+texTransform.sizeDelta.y * positions[i].y,
                    texTransform.position.z
                );
                children[i].AdjustChildrenPositions();
            }
        }
    }

    #endregion

    #region Display

    public void DisplayDepth()
    {
        image.texture = depthTex;
        if(children != null)
            foreach (var c in children) c.DisplayDepth();
    }

    public void DisplayNormal()
    {
        image.texture = normalTex;
        if (children != null)
            foreach (var c in children) c.DisplayNormal();
    }

    #endregion
}
