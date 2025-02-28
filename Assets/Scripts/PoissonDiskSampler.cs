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


[RequireComponent(typeof(BoxCollider))]
public class PoissonDiskSampler : MonoBehaviour
{
    public BoxCollider PoissonCollider;
    public List<GameObject> Samples;
    
    [Range(0, 1f)]public float SpawnSpeed;
    [Range(0.1f, 1f)] public float SamplingRadius;
    [Range(5, 30)] public int SampleLimit;

    private const string GROUND_LAYER = "Ground";
    private const float Y_OFFSET_FOR_SAMPLING_RAYCAST = 10f;
    private const int BATCH_LIMIT = 100;

    public List<Vector3?> _activePoints = new List<Vector3?>();
    [HideInInspector]public List<Vector3> _finalPoints = new List<Vector3>();

    private CancellationTokenSource _mainSamplingCancellationTokenSource;
    public CancellationToken MainSampleCancellationToken;

    private void Start()
    {
        DoSampling();
    }

    public void DoSampling()
    {
        _mainSamplingCancellationTokenSource = new CancellationTokenSource();
        MainSampleCancellationToken = _mainSamplingCancellationTokenSource.Token;
        PoissonInstance poissonInstance = new PoissonInstance(this);
        poissonInstance.Async_DoSampling();
    }
    
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