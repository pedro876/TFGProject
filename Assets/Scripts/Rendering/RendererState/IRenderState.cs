using System.Collections;
using UnityEngine;

public interface IRenderState 
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

    IRenderState[] Children { get; }

}
