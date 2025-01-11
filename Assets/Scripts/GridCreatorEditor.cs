using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProceduralGenerator))]
public class GridCreatorEditor : Editor
{
    private ProceduralGenerator _proceduralGenerator;
    public override void OnInspectorGUI()
    {
        _proceduralGenerator = (ProceduralGenerator)target;
        DrawDefaultInspector();
        if(GUILayout.Button("Generate Perlin Mesh"))
            _proceduralGenerator.DoCreateGrid();
    }
}
