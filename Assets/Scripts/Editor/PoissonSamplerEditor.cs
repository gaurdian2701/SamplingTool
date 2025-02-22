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
    }
}
