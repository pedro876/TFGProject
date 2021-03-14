using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Init : MonoBehaviour
{
    [SerializeField] float garbageCollectionInterval = 30f;

    private void Start()
    {
        QualitySettings.vSyncCount = 1;
        StartCoroutine(CollectGarbage());
    }

    IEnumerator CollectGarbage()
    {
        while (true)
        {
            yield return new WaitForSeconds(garbageCollectionInterval);
            System.GC.Collect();
        }
    }
}
