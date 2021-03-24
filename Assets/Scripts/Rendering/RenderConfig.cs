using System.Collections;
using UnityEngine;

namespace RenderingSpace
{
    public class RenderConfig
    {
        #region Performance
        public static readonly float targetFramerate = 30f;
        public static readonly int displayOrdersPerFrame = 1;
        public static readonly float renderOrdersInterval = 0.05f;
        public static int maxParallelThreads = 10;
        #endregion

        #region Exploration
        public static readonly int gpuHomogeneityDepth = 90;
        public static readonly int explorationSamples = 10;
        public static readonly float depthExplorationMultiplier = 1.05f;
        public static readonly float normalExplorationMultiplier = 0.8f;
        public static readonly float normalPlaneMultiplier = 0.08f;
        #endregion

        #region Homogeneity
        public static readonly int homogeneityPoints = 10;
        #endregion

        #region Setting
        public static readonly QuadLevel[] cpuSetting = new QuadLevel[]
            {
                new QuadLevel(64, 90),
                new QuadLevel(128, 128),
                new QuadLevel(128, 200),
                new QuadLevel(128, 1024),
            };
        public static readonly QuadLevel[] gpuSetting = new QuadLevel[]
            {
                new QuadLevel(512, 512),
                new QuadLevel(512, 1024),
                //new QuadLevel(360, 1024),
            };
        #endregion
    }
}