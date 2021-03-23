using System.Collections;
using UnityEngine;

namespace RenderingSpace
{
    public interface IRenderer
    {
        #region State
        bool IsDeepFinished { get; }
        bool IsQueued { get; }
        bool IsRendering { get; }
        bool IsDone { get; }
        bool IsRenderingChildren { get; }
        bool IsTextureApplied { get; }
        #endregion

        #region Homogeneity
        float Homogeneity { get; }
        #endregion

        void Update();
        void Destroy();
        void Stop();
        void Render();
        void DisplayDepth();
        void DisplayNormals();
        IRenderer GetChild(int childIndex);
    }
}