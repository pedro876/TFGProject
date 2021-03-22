using System.Collections;
using UnityEngine;

namespace RenderingSpace
{
    public class Ray
    {
        private Vector3 origin;
        private Vector3 destiny;
        private int samples;
        private float[] mem;
        private IMassFacade massFacade;
        private IFuncFacade funcFacade;
        bool depthOnly;

        public Ray(int samples)
        {
            this.funcFacade = ServiceLocator.Instance.GetService<IFuncFacade>();
            this.massFacade = ServiceLocator.Instance.GetService<IMassFacade>();
            this.depthOnly = false;
            this.samples = samples;
            this.mem = funcFacade.GetBytecodeMemCopy();
        }

        public void SetDepthOnly()
        {
            depthOnly = true;
        }

        public void SetOriginAndDestiny(ref Vector3 origin, ref Vector3 destiny)
        {
            this.origin = origin;
            this.destiny = destiny;
        }

        public void Cast(out bool landed, out float normDepth, out Color normalColor)
        {
            normDepth = 0f;
            landed = false;
            Vector3 pos = Vector3.zero;
            for (int z = 0; z < samples && !landed; z++)
            {
                normDepth = (float)z / (samples - 1);
                pos = Vector3.Lerp(origin, destiny, normDepth);
                landed = IsMass(ref pos, mem);
            }

            normalColor = Color.white;
            if (landed && !depthOnly)
            {
                normDepth = ReachSurfaceAndCalculateNormal(pos, out Vector3 normal);
                normalColor = new Color(normal.x + 0.5f, normal.z + 0.5f, normal.y + 0.5f, 1f);
            }
        }

        private float ReachSurfaceAndCalculateNormal(Vector3 pos, out Vector3 normal)
        {
            Vector3 rayDir = (origin - destiny);
            float rayDirMag = rayDir.magnitude;
            float rayStep = rayDirMag / samples;
            rayDir.Normalize();

            Vector3 surface = ExploreDirectionDAC(pos, pos + rayDir * rayStep * RenderConfig.depthExplorationMultiplier, mem, true);
            float normDepth = Vector3.Distance(origin, surface) / rayDirMag;

            normal = CalculateNormal(ref surface, ref rayDir, rayStep, mem) * 0.5f;
            return normDepth;
        }

        private Vector3 ExploreDirectionDAC(Vector3 origin, Vector3 destiny, float[] mem, bool originInside)
        {
            Vector3 middle = destiny;

            bool reachedSurface = IsMass(ref destiny, mem) != originInside;
            if (reachedSurface)
            {
                for (int i = 0; i < RenderConfig.explorationSamples; i++)
                {
                    middle = Vector3.Lerp(origin, destiny, 0.5f);
                    if (IsMass(ref middle, mem) == originInside)
                        origin = middle;
                    else
                        destiny = middle;
                }
            }

            return middle;
        }

        private Vector3 CalculateNormal(ref Vector3 pos, ref Vector3 up, float explorationRadius, float[] mem)
        {
            Vector3 n = Vector3.zero;

            Vector3 right = new Vector3(1, 1, (-up.x - up.y) / up.z);
            right.Normalize();
            Vector3 forward = Vector3.Cross(up, right);
            forward.Normalize();

            Vector3[] points = new Vector3[]
            {
                pos+right*explorationRadius*RenderConfig.normalPlaneMultiplier,
                pos+forward*explorationRadius*RenderConfig.normalPlaneMultiplier,
                pos-right*explorationRadius*RenderConfig.normalPlaneMultiplier,
                pos-forward*explorationRadius*RenderConfig.normalPlaneMultiplier,
            };

            up = up.normalized * explorationRadius * RenderConfig.normalExplorationMultiplier;

            for (int i = 0; i < points.Length; i++)
            {
                bool pointInside = IsMass(ref points[i], mem);
                Vector3 s = ExploreDirectionDAC(points[i], points[i] + (pointInside ? up : -up), mem, pointInside);
                points[i] = (s - pos).normalized;
            }

            for (int i = 0; i < points.Length - 1; i++)
                n += Vector3.Cross(points[i], points[i + 1]);

            n.Normalize();


            return n;
        }

        private bool IsMass(ref Vector3 pos, float[] mem)
        {
            float eval = funcFacade.Solve(pos.x, pos.y, pos.z, mem);
            return massFacade.IsMass(pos.x, pos.y, pos.z, eval);
        }
    }
}