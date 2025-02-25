using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[RequireComponent(typeof(BoxCollider))]
public class PoissonDiskSampler : MonoBehaviour
{
    public BoxCollider PoissonCollider;
    public List<GameObject> Samples;

    [Range(0.1f, 1f)] public float SamplingRadius;
    [Range(5, 30)] public int SampleLimit;

    private const string GROUND_LAYER = "Ground";
    private const float Y_OFFSET_FOR_SAMPLING_RAYCAST = 1000f;
    private const int BATCH_LIMIT = 100;

    private List<Vector3?> _activePoints = new List<Vector3?>();
    private List<Vector3> _finalPoints = new List<Vector3>();

    private void Start()
    {
        DoSampling();
    }

    public void DoSampling()
    {
        StartCoroutine(nameof(Coroutine_SamplingCoroutine));
    }

    private IEnumerator Coroutine_SamplingCoroutine()
    {
        //Get a random point on the mesh
        Vector3? randomPointOnMesh = GetRandomPointOnMesh();
        if (randomPointOnMesh == null)
        {
            Debug.LogWarning(
                "Could not find random point on mesh. Please move the collider to an appropriate position.");
            yield break;
        }

        _activePoints.Add(randomPointOnMesh);

        while (_activePoints.Count > 0)
        {
            yield return DoSamplingBatch();
            yield return new WaitForSecondsRealtime(3f);
        }
    }

    private IEnumerator DoSamplingBatch()
    {
        sbyte currentIterationForBatch = 0;

        while (currentIterationForBatch < BATCH_LIMIT)
        {
            Vector3? sampledPoint = _activePoints[Random.Range(0, _activePoints.Count)];
            Vector3? sampleNeighbourPointReturnedIfPointIsValid = null;
            if (PointIsValid(sampledPoint.Value, ref sampleNeighbourPointReturnedIfPointIsValid))
            {
                currentIterationForBatch++;
                _finalPoints.Add(sampleNeighbourPointReturnedIfPointIsValid.Value);
                _activePoints.Add(sampleNeighbourPointReturnedIfPointIsValid);
                InstantiateAtPoint(sampleNeighbourPointReturnedIfPointIsValid.Value);
            }
            else
                _activePoints.Remove(sampledPoint);

            yield return new WaitForSecondsRealtime(0.3f);
        }
    }

    private void InstantiateAtPoint(Vector3 point)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = point;
        cube.transform.localScale /= 10f;
        Samples.Add(cube);
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

    private bool PointIsValid(Vector3 point, ref Vector3? samplePointReturnedIfValidPoint)
    {
        RaycastHit hitInfo = new RaycastHit();
        for (int i = 0; i < SampleLimit; i++)
        {
            Vector3 neighbourPoint = GetRandomPointOnDisk(point);
            if (!TryRayCast(neighbourPoint, Vector3.zero, ref hitInfo)) continue;
            if (PointIsAtAppropriateDistance(neighbourPoint))
            {
                neighbourPoint.y = hitInfo.point.y;
                samplePointReturnedIfValidPoint = neighbourPoint;
                return true;
            }
        }

        return false;
    }

    private Vector3 GetRandomPointOnDisk(Vector3 point)
    {
        //Getting a random point on the disk surrounding the sampled point
        float randomAngleAroundSampledPoint = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 randomDirectionalVector =
            new Vector3(Mathf.Cos(randomAngleAroundSampledPoint), 0f, Mathf.Sin(randomAngleAroundSampledPoint))
                .normalized *
            Random.Range(SamplingRadius, SamplingRadius * 2f);
        return point + randomDirectionalVector;
    }

    private bool PointIsAtAppropriateDistance(Vector3 neighbourPoint)
    {
        float minimalDistance = float.MaxValue;
        float distanceBetweenPoints = Mathf.NegativeInfinity;
        Vector3 minimalPoint = Vector3.zero;
        for (int i = 0; i < _activePoints.Count; i++)
        {
            distanceBetweenPoints = Vector3.Distance(_activePoints[i].Value, neighbourPoint);
            if (distanceBetweenPoints < minimalDistance)
            {
                minimalPoint = _activePoints[i].Value;
                minimalDistance = distanceBetweenPoints;
            }
        }

        Debug.Log("NEIGHBOUR POINT: " + neighbourPoint + 
                  "\nMINIMAL POINT: " + minimalPoint +
                  "\nDISTANCE BETWEEN POINTS: " + 
                  minimalDistance + 
                  "\nSAMPLING RADIUS: " + SamplingRadius);
        if (minimalDistance >= SamplingRadius)
            return true;
        return false;
    }

    private void FillSampledPoints()
    {
        for (int i = 0; i < _finalPoints.Count; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = _finalPoints[i];
            cube.transform.localScale /= 10f;
            Samples.Add(cube);
        }
    }

    public void ClearSampledPoints()
    {
        for (short i = 0; i < Samples.Count; i++)
            DestroyImmediate(Samples[i]);
        Samples.Clear();
    }

    private bool TryRayCast(Vector3 rayOrigin, Vector3 offset, ref RaycastHit hitInfo)
    {
        Vector3 originWithOffset = rayOrigin + offset;
        originWithOffset.y *= Y_OFFSET_FOR_SAMPLING_RAYCAST;
        originWithOffset.y = Mathf.Abs(originWithOffset.y);

        return Physics.Raycast(originWithOffset, Vector3.down, out hitInfo, Mathf.Infinity,
            LayerMask.GetMask(GROUND_LAYER));
    }
}