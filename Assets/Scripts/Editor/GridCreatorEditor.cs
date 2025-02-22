using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProceduralGenerator))]
public class GridCreatorEditor : Editor
{
    private ProceduralGenerator _proceduralGenerator;
    public override void OnInspectorGUI()
    {
        _proceduralGenerator = target as ProceduralGenerator;
        DrawDefaultInspector();
        if(GUILayout.Button("Generate Perlin Mesh"))
            _proceduralGenerator.DoCreateGrid();
        
        UnityEditor.EditorApplication.delayCall+=()=>
        {
            DestroyImmediate(_proceduralGenerator.TerrainMeshCollider);
            _proceduralGenerator.ResetMeshCollider();
        };
    }
}
