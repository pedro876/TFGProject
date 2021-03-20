using System.Collections;
using UnityEngine;


public class Installer : MonoBehaviour
{
    private static bool installed = false;
    private void Awake()
    {
        if (!installed)
        {
            installed = true;
            InstallServices();
        }
    }

    private void InstallServices()
    {
        ServiceLocator.Instance.RegisterService<IFuncFacade>(new FuncFacade());
    }
}
