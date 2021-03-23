using UnityEngine;
using System;

namespace RegionSpace
{
    public class RegionFacade : IRegionFacade
    {
        public event Action onChanged;

        private Vector2 regionX;
        private Vector2 regionY;
        private Vector2 regionZ;
        private Vector3 regionScale;
        private Vector3 regionCenter;
        private bool clampToRegion;

        public RegionFacade()
        {
            regionX = new Vector2(-5f, 5f);
            regionY = new Vector2(-5f, 5f);
            regionZ = new Vector2(-5f, 5f);
            regionScale = Vector3.one;
            regionCenter = Vector3.zero;
            clampToRegion = true;
            SetRegion(regionX.x, regionX.y, regionY.x, regionY.y, regionZ.x, regionZ.y);
        }

        public void ActivateRegionClamp(bool active)
        {
            clampToRegion = active;
            onChanged?.Invoke();
        }
        public bool IsRegionClamped()
        {
            return clampToRegion;
        }
        public Vector3 GetRegionScale()
        {
            return regionScale;
        }
        public Vector3 GetRegionCenter()
        {
            return regionCenter;
        }
        public Vector2 GetRegionX()
        {
            return regionX;
        }
        public Vector2 GetRegionY()
        {
            return regionY;
        }
        public Vector2 GetRegionZ()
        {
            return regionZ;
        }
        public void SetRegion(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
        {
            regionX = new Vector2(Mathf.Min(minX, maxX), Mathf.Max(minX, maxX));
            regionY = new Vector2(Mathf.Min(minY, maxY), Mathf.Max(minY, maxY));
            regionZ = new Vector2(Mathf.Min(minZ, maxZ), Mathf.Max(minZ, maxZ));

            regionScale = new Vector3(regionX.y - regionX.x, regionY.y - regionY.x, regionZ.y - regionZ.x);
            regionCenter = new Vector3((regionX.x + regionX.y) * 0.5f, (regionY.x + regionY.y) * 0.5f, (regionZ.x + regionZ.y) * 0.5f);
            onChanged?.Invoke();
        }
        public Vector3 TransformToRegion(ref Vector3 pos)
        {
            return (new Vector3(pos.x * regionScale.x, pos.z * regionScale.y, pos.y * regionScale.z)) + regionCenter;
        }
        public bool IsPosOutOfRegion(ref Vector3 pos)
        {
            if (!clampToRegion) return false;
            else return
                    pos.x < regionX.x || pos.x > regionX.y ||
                    pos.y < regionY.x || pos.y > regionY.y ||
                    pos.z < regionZ.x || pos.z > regionZ.y;
        }
    }
}
