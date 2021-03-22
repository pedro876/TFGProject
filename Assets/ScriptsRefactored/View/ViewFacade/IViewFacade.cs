using UnityEngine;
using System;

public interface IViewFacade
{
    Vector3 NearTopLeft { get; }
    Vector3 NearTopRight { get; }
    Vector3 NearBottomRight { get; }
    Vector3 NearBottomLeft { get; }
    Vector3 FarTopLeft { get; }
    Vector3 FarTopRight { get; }
    Vector3 FarBottomRight { get; }
    Vector3 FarBottomLeft { get; }
    void SetCanMove(bool focus);
    bool CanMove();

    event Action onChanged;
    void UseOrbitMove();
    void UseFlyMove();
}