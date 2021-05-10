using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RenderingSpace
{
    public abstract class QuadRenderer : IRenderer
    {

        #region State
        public bool IsDeepFinished { get; set; }
        public bool IsQueued { get; set; }
        public bool IsRendering { get; set; }
        public bool IsDone { get; set; }
        public bool IsRenderingChildren { get; set; }
        public bool IsTextureApplied { get; set; }
        #endregion

        
        public QuadLevel QuadInfo { get; set; }
        public RectTransform ImageTransform { get; private set; }
        public QuadRenderer[] Children { get; set; }

        protected float region;
        protected Vector2 start;

        protected int level;
        private QuadRenderer parent;
        private int childIndex;

        protected Texture depthTex;
        protected Texture normalTex;
        private RawImage image;

        private bool sentDisplayOrder;
        private int childRenderFinishedCount = 0;

        protected IViewFacade viewFacade;
        protected IRegionFacade regionFacade;

        private float[] homogeneities = new float[4];
        private List<float[]> randomDepths;
        protected int[,] homogeneityCoords;
        protected float[,] depths;

        #region Creation

        public QuadRenderer(RawImage image, int level = 0, int childIndex = 0, QuadRenderer parent = null)
        {
            viewFacade = ServiceLocator.Instance.GetService<IViewFacade>();
            regionFacade = ServiceLocator.Instance.GetService<IRegionFacade>();

            this.childIndex = childIndex;
            this.region = 1f / (Mathf.Pow(2f, level));
            this.sentDisplayOrder = false;
            this.image = image;
            this.ImageTransform = image.GetComponent<RectTransform>();
            this.parent = parent;
            this.level = level;

            CalculateStart();
            

            this.randomDepths = new List<float[]>(4);
            for(int i = 0; i < 4; i++)
            {
                this.randomDepths.Add(new float[RenderConfig.homogeneityPoints]);
            }
            homogeneityCoords = new int[4, RenderConfig.homogeneityPoints*2];
        }

        private void CalculateStart()
        {
            if (parent != null)
            {
                start = parent.start + parent.region * positions[childIndex];
            }
            else
            {
                start = new Vector2(0, 1);
            }
            //Debug.Log($"l:{level} r:{region} s:{start}");
        }

        public void Init()
        {
            depths = new float[QuadInfo.resolution, QuadInfo.resolution];
            InitConcrete();
        }

        protected abstract void InitConcrete();

        private static readonly Vector2[] positions = new Vector2[]
        {
            new Vector2(0f,0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, -0.5f),
            new Vector2(0.5f, -0.5f),
        };

        public IRenderer GetChild(int index)
        {
            if (Children != null && index >= 0 && index < Children.Length)
            {
                return Children[index];
            }
            else return null;
        }

        #endregion

        #region Update

        public void Update()
        {
            UpdateChildren();
            CheckRenderState();
        }

        private void CheckRenderState()
        {
            if (level == 0 && !IsRenderingChildren && IsDone && !RenderingFacade.Instance.UpdateInRealTime)
            {
                RenderChildren();
            }
            if (!sentDisplayOrder && IsRendering && IsDone && !RenderingFacade.Instance.UpdateInRealTime && level > 0)
            {
                sentDisplayOrder = true;
                RenderingFacade.Instance.displayOrders.Enqueue(() =>
                {
                    if (level > 0) MemoryToTex();
                    IsRendering = false;
                    RenderChildren();
                });
            }
        }

        protected void MemoryToTex()
        {
            if(image)image.enabled = true;
            IsTextureApplied = true;
            MemoryToTexConcrete();
            RenderingFacade.Instance.TextureHasBeenApplied();
        }

        protected abstract void MemoryToTexConcrete();

        private void UpdateChildren()
        {
            if(Children != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    Children[i].Update();
                }
                AdjustChildrenPosition();
            }
            
        }

        private void AdjustChildrenPosition()
        {
            for (int i = 0; i < 4; i++)
            {
                Children[i].ImageTransform.sizeDelta = new Vector2(ImageTransform.sizeDelta.x * 0.5f, ImageTransform.sizeDelta.y * 0.5f);
                Children[i].ImageTransform.position = new Vector3(
                    ImageTransform.position.x + ImageTransform.sizeDelta.x * positions[i].x,
                    ImageTransform.position.y + ImageTransform.sizeDelta.y * positions[i].y,
                    ImageTransform.position.z
                );
            }
        }

        #endregion

        #region Rendering

        protected void GetCameraInfo(out Vector3 right, out Vector3 up, out float nearSize, out float farSize, out Vector3 nearStart, out Vector3 farStart)
        {
            right = (viewFacade.NearTopRight - viewFacade.NearTopLeft).normalized;
            up = (viewFacade.NearTopLeft - viewFacade.NearBottomLeft).normalized;
            nearSize = Vector3.Distance(viewFacade.NearTopLeft, viewFacade.NearTopRight);
            farSize = Vector3.Distance(viewFacade.FarTopLeft, viewFacade.FarTopRight);
            nearStart = Vector3.Lerp(viewFacade.NearTopLeft, viewFacade.NearTopRight, start.x) - up * (1f - start.y) * nearSize;
            farStart = Vector3.Lerp(viewFacade.FarTopLeft, viewFacade.FarTopRight, start.x) - up * (1f - start.y) * farSize;
        }

        public void Stop()
        {
            image.enabled = level == 0;
            IsDone = false;
            IsRenderingChildren = false;
            IsQueued = false;
            IsRendering = false;
            IsTextureApplied = false;
            IsDeepFinished = false;
            sentDisplayOrder = false;
            if(Children != null)
            {
                foreach (var child in Children)
                    child.Stop();
            }
        }

        public void Render()
        {
            IsDone = false;
            IsRendering = true;
            childRenderFinishedCount = 0;

            if (level > 0 && image) image.enabled = false;

            CalculateHomogeneityPoints();
            if (level == 0)
            {
                RenderInstant();
            }
            else
            {
                IsQueued = true;
                int priority = Mathf.RoundToInt(Importance * 100);
                if (RenderConfig.maxParallelThreads > 0)
                    RenderMultiThread(priority);
                else
                    RenderSingleThread(priority);
            }
        }
        protected abstract void RenderInstant();
        protected abstract void RenderSingleThread(int priority);
        protected abstract void RenderMultiThread(int priority);

        private void RenderChildren()
        {
            CalculateHomogeneity();
            if (parent != null)
            {
                parent.childRenderFinishedCount++;
                if (parent.childRenderFinishedCount >= 4)
                    parent.image.enabled = false;
            }
            if (Children != null)
            {
                IsRenderingChildren = true;
                foreach (var c in Children) c.Render();
            }
            else
                RenderFinishedSignal();
        }

        private void RenderFinishedSignal()
        {
            IsDeepFinished = true;
            if (Children != null)
            {
                foreach (QuadRenderer r in Children)
                    if (!r.IsDeepFinished)
                        IsDeepFinished = false;
            }

            if (IsDeepFinished && level > 0) parent.RenderFinishedSignal();
        }

        #endregion

        #region Homogeneity

        public float Homogeneity => parent != null ? parent.homogeneities[childIndex] : 0f;
        protected float Disparity => 1f - Homogeneity;
        protected float Importance => Disparity + (RenderingFacade.Instance.QuadTreeDepth - level);

        protected void GetSubRegion(int r, out int minX, out int minY, out int maxX, out int maxY)
        {
            minX = (r == 0 || r == 2) ? 0 : QuadInfo.resolution / 2;
            minY = (r == 0 || r == 1) ? 0 : QuadInfo.resolution / 2;
            maxX = (r == 0 || r == 2) ? QuadInfo.resolution / 2 : QuadInfo.resolution;
            maxY = (r == 0 || r == 1) ? QuadInfo.resolution / 2 : QuadInfo.resolution;
        }

        private void CalculateHomogeneityPoints()
        {
            for (int r = 0; r < 4; r++)
            {
                GetSubRegion(r, out int minX, out int minY, out int maxX, out int maxY);
                SetHomogeneityCoord(r, 0, minX, minY);
                SetHomogeneityCoord(r, 1, minX, maxY - 1);
                SetHomogeneityCoord(r, 2, maxX - 1, minY);
                SetHomogeneityCoord(r, 3, maxX - 1, maxY - 1);

                for (int j = 4; j < RenderConfig.homogeneityPoints; j++)
                {
                    SetHomogeneityCoord(r, j, Random.Range(minX, maxX), Random.Range(minY, maxY));
                }
            }
        }

        private void SetHomogeneityCoord(int region, int coord, int x, int y)
        {
            homogeneityCoords[region, coord * 2 + 0] = x;
            homogeneityCoords[region, coord * 2 + 1] = y;
        }

        private void CalculateHomogeneity()
        {
            for (int i = 0; i < 4; i++)
            {
                GetRandomDepths(i, randomDepths[i]);
            }
            for (int i = 0; i < homogeneities.Length; i++) homogeneities[i] = 0f;

            float totalDistances = 0f;
            for (int i = 0; i < RenderConfig.homogeneityPoints; i++)
            {
                for (int j = i; j < RenderConfig.homogeneityPoints; j++)
                {
                    homogeneities[0] += Mathf.Abs(randomDepths[0][i] - randomDepths[0][j]);
                    homogeneities[1] += Mathf.Abs(randomDepths[1][i] - randomDepths[1][j]);
                    homogeneities[2] += Mathf.Abs(randomDepths[2][i] - randomDepths[2][j]);
                    homogeneities[3] += Mathf.Abs(randomDepths[3][i] - randomDepths[3][j]);
                    totalDistances++;
                }
            }

            for (int i = 0; i < homogeneities.Length; i++)
                homogeneities[i] = 1f - Mathf.Clamp(homogeneities[i] / totalDistances, 0f, 1f);
        }

        private void GetRandomDepths(int r, float[] homogeneityDepths)
        {
            for (int i = 0; i < RenderConfig.homogeneityPoints; i++)
            {
                int x = homogeneityCoords[r, i * 2 + 0];
                int y = homogeneityCoords[r, i * 2 + 1];
                homogeneityDepths[i] = depths[x, y];
            }
        }

        #endregion

        #region Display

        public void DisplayDepth()
        {
            image.texture = depthTex;
            if (Children != null)
                foreach (var c in Children)
                    c.DisplayDepth();
        }
        public void DisplayNormals()
        {
            image.texture = normalTex;
            if (Children != null)
                foreach (var c in Children)
                    c.DisplayNormals();
        }

        #endregion

        #region Destroy

        public void Destroy()
        {
            if(level == 0 && image != null)
            {
                Object.DestroyImmediate(image.gameObject);
            }
            
            if (Children != null)
            {
                for (int i = 0; i < 4; i++)
                    Children[i].Destroy();
            }
            Finish();
        }

        protected abstract void Finish();

        #endregion
    }
}