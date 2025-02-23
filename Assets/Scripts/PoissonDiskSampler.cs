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
    
    private List<Vector3?> _activePoints = new List<Vector3?>();

    public void DoSampling()
    {
        //Get a random point on the mesh
        Vector3? randomPointOnMesh = GetRandomPointOnMesh();
        if (randomPointOnMesh == null)
        {
            Debug.LogWarning("Could not find random point on mesh. Please move the collider to an appropriate position.");
            return;
        }
        _activePoints.Add(randomPointOnMesh);

        while(_activePoints.Count > 0)
        {
            Vector3? sampledPoint = _activePoints[Random.Range(0, _activePoints.Count)];
        }
    }

    private Vector3? GetRandomPointOnMesh()
    {
        Vector3 colliderExtentsOffset = new Vector3(Random.Range(0, PoissonCollider.bounds.extents.x), 
            PoissonCollider.bounds.extents.y,
            Random.Range(0, PoissonCollider.bounds.extents.z));
        RaycastHit hitInfo = new RaycastHit();
        if (TryRayCast(transform.position, colliderExtentsOffset, ref hitInfo))
            return hitInfo.point;
        return null;
    }

    private bool PointIsValid(Vector3 point)
    {
        RaycastHit hitInfo = new RaycastHit();
        for (sbyte i = 0; i < SampleLimit; i++)
        {
            Vector3 sampledPoint = GetRandomPointOnDisk(point);
            if (!TryRayCast(sampledPoint, Vector3.zero, ref hitInfo)) continue;
            return false;
        }

        return false;
    }

    private Vector3 GetRandomPointOnDisk(Vector3 point)
    {
        //Getting a random point on the disk surrounding the sampled point
        float randomAngleAroundSampledPoint = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 randomDirectionalVector = new Vector3(Mathf.Cos(randomAngleAroundSampledPoint), 0f, Mathf.Sin(randomAngleAroundSampledPoint)).normalized *
                                          Random.Range(SamplingRadius, SamplingRadius * 2f);
        return point + randomDirectionalVector;
    }

    private bool TryRayCast(Vector3 rayOrigin, Vector3 offset, ref RaycastHit hitInfo)
    {
        Vector3 originWithOffset = rayOrigin + offset;
        originWithOffset.y *= Y_OFFSET_FOR_SAMPLING_RAYCAST;
        return Physics.Raycast(originWithOffset, Vector3.down, out hitInfo, Mathf.Infinity, LayerMask.GetMask(GROUND_LAYER));
    }
}
