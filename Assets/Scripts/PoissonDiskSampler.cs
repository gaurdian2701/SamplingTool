using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

[ExecuteInEditMode]
[RequireComponent(typeof(BoxCollider))]
public class PoissonDiskSampler : MonoBehaviour
{
    public BoxCollider PoissonCollider;
    public List<GameObject> Samples;
    
    [Range(0, 1f)]public float SpawnSpeed;
    [Range(0.1f, 1f)] public float SamplingRadius;
    [Range(5, 30)] public int SampleLimit;
    [Tooltip("The number of times the bounding area is divided into quadrants to facilitate asynchronous sampling. For example, a value of 1 " +
            "means that the bounding area will be divided into 4 quadrants with each quadrant having it's own sampling process running asynchronously.")]
    [Range(1, 10)]public int MaxNumberOfSubdivisions;

    private const string GROUND_LAYER = "Ground";
    private const float Y_OFFSET_FOR_SAMPLING_RAYCAST = 10f;
    private const int BATCH_LIMIT = 100;

    private CancellationTokenSource _mainSamplingCancellationTokenSource;
    public CancellationToken MainSampleCancellationToken;
    
    //USE THIS AS A MAIN THREAD DISPATCHER

    public void DoAsyncSampling()
    {
        _mainSamplingCancellationTokenSource = new CancellationTokenSource();
        MainSampleCancellationToken = _mainSamplingCancellationTokenSource.Token;
        CreateBuckets(transform.position, PoissonCollider.bounds.extents);
    }

    public void DoSimpleSampling()
    {
        _mainSamplingCancellationTokenSource = new CancellationTokenSource();
        MainSampleCancellationToken = _mainSamplingCancellationTokenSource.Token;
        PoissonInstance poissonInstance = new PoissonInstance(this, transform.position, 
            PoissonCollider.bounds.extents, MainSampleCancellationToken);
        poissonInstance.Async_DoSampling();
    }

    private void CreateBuckets(Vector3 centerOfBoundingArea, Vector3 extentsOfBoundingArea)
    {
        if (Mathf.Approximately(2 * extentsOfBoundingArea.x,
                2 * PoissonCollider.bounds.extents.x / Mathf.Pow(2, MaxNumberOfSubdivisions)))
        {
            PoissonInstance poissonInstance = new PoissonInstance(this, centerOfBoundingArea, extentsOfBoundingArea, MainSampleCancellationToken);
            poissonInstance.Async_DoSampling();
            return;
        }
        
        CreateBuckets(new Vector3(centerOfBoundingArea.x - extentsOfBoundingArea.x/2, centerOfBoundingArea.y, 
                centerOfBoundingArea.z + extentsOfBoundingArea.z/2), GetHalfExtents(extentsOfBoundingArea)); //Upper Left Quadrant
        CreateBuckets(new Vector3(centerOfBoundingArea.x + extentsOfBoundingArea.x/2, centerOfBoundingArea.y,
            centerOfBoundingArea.z + extentsOfBoundingArea.z/2), GetHalfExtents(extentsOfBoundingArea)); //Upper Right Quadrant
        CreateBuckets(new Vector3(centerOfBoundingArea.x - extentsOfBoundingArea.x/2, centerOfBoundingArea.y,
            centerOfBoundingArea.z - extentsOfBoundingArea.z/2), GetHalfExtents(extentsOfBoundingArea));  //Lower Left Quadrant
        CreateBuckets(new Vector3(centerOfBoundingArea.x + extentsOfBoundingArea.x/2, centerOfBoundingArea.y,
            centerOfBoundingArea.z - extentsOfBoundingArea.z/2), GetHalfExtents(extentsOfBoundingArea)); //Lower Right Quadrant
    }

    private Vector3 GetHalfExtents(Vector3 currentExtents) => new Vector3(currentExtents.x / 2, currentExtents.y, currentExtents.z / 2);
    public void ClearSampledPoints()
    {
        for (short i = 0; i < Samples.Count; i++)
            DestroyImmediate(Samples[i]);
        Samples.Clear();
    }

    public void StopSampling()
    {
        _mainSamplingCancellationTokenSource.Cancel();
    }
}