using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarbageCollectorController : MonoBehaviour
{
    [SerializeField] float garbageCollectionInterval = 30f;

    private void Start()
    {
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
