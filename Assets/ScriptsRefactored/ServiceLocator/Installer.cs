﻿using System.Collections;
using UnityEngine;


public class Installer : MonoBehaviour
{
    private static bool installed = false;
    private void Awake()
    {
        QualitySettings.vSyncCount = 1;

        if (!installed)
        {
            installed = true;
            InstallServices();
        }
    }

    private void InstallServices()
    {

        ServiceLocator.Instance.RegisterService<IFuncFacade>(new FuncSpace.FuncFacade());
        ServiceLocator.Instance.RegisterService<IViewFacade>(FindObjectOfType<ViewSpace.ViewFacade>());
        ServiceLocator.Instance.RegisterService<IRegionFacade>(new RegionSpace.RegionFacade());
        ServiceLocator.Instance.RegisterService<IMassFacade>(new MassSpace.MassFacade());
        ServiceLocator.Instance.RegisterService<IRenderingFacade>(FindObjectOfType<RenderingSpace.RenderingFacade>());
        ServiceLocator.Instance.RegisterService<IPostProcessFacade>(FindObjectOfType<PostProcessSpace.PostProcessFacade>());
        ServiceLocator.Instance.RegisterService<ILightingFacade>(new LightingSpace.LightingFacade());
    }
}
