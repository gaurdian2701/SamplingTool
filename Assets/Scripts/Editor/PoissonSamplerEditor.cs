using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PoissonDiskSampler))]
public class PoissonSamplerEditor : Editor
{
    private PoissonDiskSampler _poissonDiskSampler;

    public override void OnInspectorGUI()
    {
        _poissonDiskSampler = target as PoissonDiskSampler;
        DrawDefaultInspector();
        if(GUILayout.Button("Start Sampling"))
            _poissonDiskSampler.DoSampling();
        if(GUILayout.Button("Clear Samples"))
            _poissonDiskSampler.ClearSampledPoints();
        if(GUILayout.Button("Stop Sampling"))
            _poissonDiskSampler.StopSampling();
    }
}
