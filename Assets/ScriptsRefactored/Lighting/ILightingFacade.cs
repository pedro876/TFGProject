using UnityEngine;
using System;


public interface ILightingFacade
{
    Vector2 GetLightDir();
    Vector3 GetLightDirVec();
    void SetLightDir(float rotation, float inclination);
    void ActivateFog(bool fog);
    bool IsFogActive();
    float GetFogPower();
    void SetFogPower(float power);
    event Action onChanged;
}
