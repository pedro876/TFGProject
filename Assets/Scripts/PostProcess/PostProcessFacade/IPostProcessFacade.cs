using System;
using UnityEngine;

public interface IPostProcessFacade
{
    event Action onDisplayUpdated;
    event Action onRendered;
    RenderTexture DisplayTexture { get; }
    void DisplayDepth();
    void DisplayNormals();
    void DisplayLighting();
    Texture2D GetDisplayTextureCopy();
    void UseAntialiasing(bool use);
    bool IsUsingAntialiasing();
    bool IsDisplayinDepth();
    bool IsDisplayinNormals();
    bool IsDisplayinLighting();
}
