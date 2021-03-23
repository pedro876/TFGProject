using System;
using UnityEngine;

public interface IPostProcessFacade
{
    event Action onDisplayUpdated;
    RenderTexture DisplayTexture { get; }
    void DisplayDepth();
    void DisplayNormals();
    void DisplayLighting();
    void UseAntialiasing(bool use);
    bool IsUsingAntialiasing();
    bool IsDisplayinDepth();
    bool IsDisplayinNormals();
    bool IsDisplayinLighting();
}
