using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[RequireComponent(typeof(BoxCollider))]
public class PoissonDiskSampler : MonoBehaviour
{
    [SerializeField] private BoxCollider PoissonCollider;
    
    public int SamplingRadius;
    public int SampleLimit;
    
    private const string GROUND_LAYER = "Ground";
    private const float Y_OFFSET_FOR_SAMPLING_RAYCAST = 2f;

    public void DoSampling()
    {
        //Get a random point on the mesh
        GetRandomPointOnMesh();
    }

    private void GetRandomPointOnMesh()
    {
        Vector3 colliderExtentsOffset = new Vector3(Random.Range(0, PoissonCollider.bounds.extents.x), 
            PoissonCollider.bounds.extents.y,
            Random.Range(0, PoissonCollider.bounds.extents.z));
        Vector3 rayCastOriginPoint = transform.position + colliderExtentsOffset;
        rayCastOriginPoint.y *= Y_OFFSET_FOR_SAMPLING_RAYCAST;
        Physics.Raycast(rayCastOriginPoint, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask(GROUND_LAYER));
        Debug.Log(hit.point);
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = hit.point;
        cube.transform.localScale /= 10;
    }
}
