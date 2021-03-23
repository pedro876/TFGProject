using System.Collections;
using UnityEngine;

namespace RenderingSpace
{
    public class RenderState : IRenderState
    {
        #region State
        public bool IsDeepFinished { get; private set; }
        public bool IsQueued { get; private set; }
        public bool IsRendering { get; private set; }
        public bool IsDone { get; private set; }
        public bool IsRenderingChildren { get; private set; }
        public bool IsTextureApplied { get; private set; }
        #endregion

        #region Homogeneity
        public float Homogeneity { get; private set; }
        #endregion

        #region Children
        public IRenderState[] Children { get; private set; }
        #endregion

        public RenderState(QuadLevel[] setting, int level = 0)
        {
            if(level < setting.Length - 1)
            {
                Children = new IRenderState[4];
                for(int i = 0; i < 4; i++)
                {
                    Children[i] = new RenderState(setting, level + 1);
                }
            } else
            {
                Children = null;
            }
        }

        public void ExtractStateFromRenderer(IRenderer renderer)
        {
            IsDeepFinished = renderer.IsDeepFinished;
            IsQueued = renderer.IsQueued;
            IsRendering = renderer.IsRendering;
            IsDone = renderer.IsDone;
            IsRenderingChildren = renderer.IsRenderingChildren;
            IsTextureApplied = renderer.IsTextureApplied;
            Homogeneity = renderer.Homogeneity;
            
            if(Children != null)
            {
                for(int i = 0; i < Children.Length; i++)
                {
                    ((RenderState)Children[i]).ExtractStateFromRenderer(renderer.GetChild(i));
                }
            }
        }
    }
}