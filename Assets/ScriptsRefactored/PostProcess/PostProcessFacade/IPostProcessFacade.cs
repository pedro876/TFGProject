using System;
using UnityEngine;

public interface IPostProcessFacade
{
    event Action<RenderTexture> onDisplayUpdated;
    void DisplayDepth();
    void DisplayNormals();
    void DisplayLighting();
    void UseAntialiasing(bool use);
    bool IsUsingAntialiasing();
    bool IsDisplayinDepth();
    bool IsDisplayinNormals();
    bool IsDisplayinLighting();
}
