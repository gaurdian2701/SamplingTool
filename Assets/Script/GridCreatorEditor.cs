using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridCreator))]
public class GridCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GridCreator gridCreator = (GridCreator)target;
        DrawDefaultInspector();
        if(GUILayout.Button("Generate Perlin Mesh"))
            gridCreator.DoCreateGrid();
    }
}
