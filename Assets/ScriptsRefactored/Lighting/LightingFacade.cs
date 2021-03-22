using UnityEngine;
using System;

namespace LightingSpace
{
    public class LightingFacade : ILightingFacade
    {
        public event Action onChanged;

        private bool fog;
        private float fogPower;
        private Vector3 lightDir;

        public LightingFacade()
        {
            fog = true;
            fogPower = 3f;
            lightDir = new Vector3(-0.5f, -1.3f, 0f).normalized;
        }

        public Vector2 GetLightDir()
        {
            lightDir.Normalize();
            Vector3 projUp = Vector3.ProjectOnPlane(lightDir, Vector3.up).normalized;
            float rotation = Vector3.SignedAngle(Vector3.right, projUp, Vector3.up);
            rotation = rotation % 360f;
            while (rotation < 0f)
                rotation += 360f;

            Vector3 axis = Vector3.Cross(lightDir, projUp);
            float inclination = Vector3.SignedAngle(projUp, lightDir, axis);

            while (inclination > 90f)
                inclination -= 180f;
            while (inclination < -90f)
                inclination += 180f;

            return new Vector2(rotation, inclination);
        }
        public Vector3 GetLightDirVec()
        {
            return lightDir;
        }
        public void SetLightDir(float rotation, float inclination)
        {
            lightDir = Vector3.right;
            lightDir = Quaternion.AngleAxis(inclination, Vector3.forward) * lightDir;
            lightDir = Quaternion.AngleAxis(rotation, Vector3.up) * lightDir;
            lightDir.Normalize();
            onChanged?.Invoke();
        }
        public void ActivateFog(bool fog)
        {
            this.fog = fog;
            onChanged?.Invoke();
        }
        public bool IsFogActive()
        {
            return fog;
        }
        public float GetFogPower()
        {
            return fogPower;
        }
        public void SetFogPower(float power)
        {
            fogPower = power;
            onChanged?.Invoke();
        }
    }
}

