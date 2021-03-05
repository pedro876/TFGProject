using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalTest : MonoBehaviour
{
    [SerializeField] Transform[] surfaces;
    [SerializeField] Transform origin, projection;
    Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition);
        projection.position = new Vector3(pos.x, 0f, pos.z);
        Vector3[] projections = new Vector3[surfaces.Length];
        int closest = 0;
        float minDist = 10000f;
        for(int i = 0; i < projections.Length; i++)
        {
            projections[i] = Vector3.Project(projection.position, (surfaces[i].position - origin.position).normalized);
            float dist = (projection.position - projections[i]).magnitude;
            if(dist < minDist)
            {
                minDist = dist;
                closest = i;
            }
        }
        projection.position = projections[closest];

        float totalMag = 0f;
        float[] mags = new float[surfaces.Length];

        for(int i = 0; i < surfaces.Length; i++)
        {

        }


    }
}
