using System;
using UnityEngine;

public interface IPostProcessFacade
{
    event Action<RenderTexture> onDisplayUpdated;
    void DisplayDepth();
    void DisplayNormals();
    void DisplayLighting();
}
