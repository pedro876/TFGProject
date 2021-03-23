using UnityEngine;
using System;

public interface IRegionFacade
{
    void ActivateRegionClamp(bool active);
    bool IsRegionClamped();
    Vector3 GetRegionScale();
    Vector3 GetRegionCenter();
    Vector2 GetRegionX();
    Vector2 GetRegionY();
    Vector2 GetRegionZ();
    void SetRegion(float minX, float maxX, float minY, float maxY, float minZ, float maxZ);
    Vector3 TransformToRegion(ref Vector3 pos);
    bool IsPosOutOfRegion(ref Vector3 pos);
    event Action onChanged;
}