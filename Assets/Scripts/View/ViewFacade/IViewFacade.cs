using UnityEngine;
using System;

public interface IViewFacade
{
    float Fov { get; set; }
    float OrtographicSize { get; set; }
    bool Ortographic { get; set; }
    float Near { get; set; }
    float Far { get; set; }
    Vector3 Direction { get; }
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
    event Action onPropertyChanged;
    void UseOrbitMove();
    bool IsUsingOrbitMove();
    void UseFlyMove();
    bool IsUsingFlyMove();
}