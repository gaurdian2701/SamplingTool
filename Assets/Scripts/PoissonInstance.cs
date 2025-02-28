using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class PoissonInstance
{
    private const string GROUND_LAYER = "Ground";
    private const float Y_OFFSET_FOR_SAMPLING_RAYCAST = 10f;
    private const int BATCH_LIMIT = 100;

    private PoissonDiskSampler _poissonDiskSampler;

    public PoissonInstance(PoissonDiskSampler poissonDiskSampler)
    {
        _poissonDiskSampler = poissonDiskSampler;
    }

    public async void Async_DoSampling()
    {
        //Get a random point on the mesh
        Vector3? randomPointOnMesh = GetRandomPointOnMesh();
        if (randomPointOnMesh == null)
        {
            Debug.LogWarning(
                "Could not find random point on mesh. Please move the collider to an appropriate position.");
            return;
        }

        _poissonDiskSampler._activePoints.Add(randomPointOnMesh);
        _poissonDiskSampler._finalPoints.Add(randomPointOnMesh.Value);

        while (_poissonDiskSampler._activePoints.Count > 0)
        {
            await DoSamplingBatch();
            await Task.Delay(50);
        }
    }

    private async Task DoSamplingBatch()
    {
        sbyte currentIterationForBatch = 0;

        while (currentIterationForBatch < BATCH_LIMIT)
        {
            if (_poissonDiskSampler._activePoints.Count == 0 || _poissonDiskSampler.MainSampleCancellationToken.IsCancellationRequested)
                return;

            Vector3? sampledPoint = _poissonDiskSampler._activePoints[Random.Range(0, _poissonDiskSampler._activePoints.Count)];
            Vector3 sampleNeighbourPointReturnedIfPointIsValid = Vector3.zero;
            if (PointIsValid(sampledPoint, ref sampleNeighbourPointReturnedIfPointIsValid))
            {
                currentIterationForBatch++;
                _poissonDiskSampler._finalPoints.Add(sampleNeighbourPointReturnedIfPointIsValid);
                _poissonDiskSampler._activePoints.Add(sampleNeighbourPointReturnedIfPointIsValid);
                InstantiateAtPoint(sampleNeighbourPointReturnedIfPointIsValid);
            }
            else
                _poissonDiskSampler._activePoints.Remove(sampledPoint);

            await Task.Delay((int)(_poissonDiskSampler.SpawnSpeed * 1000));
        }
    }

    private void InstantiateAtPoint(Vector3 point)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = point;
        cube.transform.localScale /= 10f;
        _poissonDiskSampler.Samples.Add(cube);
    }

    private Vector3? GetRandomPointOnMesh()
    {
        Vector3 colliderExtentsOffset = new Vector3(
            Random.Range(0, _poissonDiskSampler.PoissonCollider.bounds.extents.x),
            _poissonDiskSampler.PoissonCollider.bounds.extents.y,
            Random.Range(0, _poissonDiskSampler.PoissonCollider.bounds.extents.z));
        RaycastHit hitInfo = new RaycastHit();
        if (TryRayCast(_poissonDiskSampler.gameObject.transform.position, colliderExtentsOffset, ref hitInfo))
            return hitInfo.point;
        return null;
    }

    private bool PointIsValid(Vector3? point, ref Vector3 samplePointReturnedIfValidPoint)
    {
        RaycastHit hitInfo = new RaycastHit();
        for (int i = 0; i < _poissonDiskSampler.SampleLimit; i++)
        {
            Vector3 neighbourPoint = GetRandomPointOnDisk(point.Value);
            if (!TryRayCast(neighbourPoint, Vector3.zero, ref hitInfo)) continue;
            if (PointIsAtAppropriateDistance(neighbourPoint) &&
                _poissonDiskSampler.PoissonCollider.bounds.Contains(neighbourPoint))
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
            Random.Range(_poissonDiskSampler.SamplingRadius, _poissonDiskSampler.SamplingRadius * 2f);
        return point + randomDirectionalVector;
    }

    private bool PointIsAtAppropriateDistance(Vector3 neighbourPoint)
    {
        float minimalDistance = float.MaxValue;
        float distanceBetweenPoints;
        Vector3 minimalPoint = Vector3.zero;
        for (int i = 0; i < _poissonDiskSampler._finalPoints.Count; i++)
        {
            distanceBetweenPoints = Vector3.Distance(_poissonDiskSampler._finalPoints[i], neighbourPoint);
            if (distanceBetweenPoints < minimalDistance)
            {
                minimalPoint = _poissonDiskSampler._finalPoints[i];
                minimalDistance = distanceBetweenPoints;
            }
        }

        if (minimalDistance >= _poissonDiskSampler.SamplingRadius)
        {
            Debug.DrawRay(neighbourPoint, Vector3.up, Color.red);
            Debug.DrawRay(minimalPoint, Vector3.up, Color.blue);
            Debug.DrawRay(neighbourPoint,
                (minimalPoint - neighbourPoint).normalized * Vector3.Distance(minimalPoint, neighbourPoint),
                Color.green);
            return true;
        }

        return false;
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