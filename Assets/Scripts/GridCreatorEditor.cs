using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridCreator))]
public class GridCreatorEditor : Editor
{
    private GridCreator gridCreator;
    public override void OnInspectorGUI()
    {
        gridCreator = (GridCreator)target;
        DrawDefaultInspector();
        if(GUILayout.Button("Generate Perlin Mesh"))
            gridCreator.DoCreateGrid();
    }
}
