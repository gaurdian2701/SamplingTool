using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GridCreation : MonoBehaviour
{
    public int Width;
    public int Height;

    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;

    private int[,] _gridData;

    private void Awake()
    {
        _gridData = new int[Width, Height];
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshFilter = GetComponent<MeshFilter>();
        GenerateMeshData();
    }

    private void GenerateMeshData()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                int numberOfVertices = vertices.Count;
                vertices.AddRange(new Vector3[]
                {
                    new Vector3(i, j, 0),
                    new Vector3(i+1, j, 0),
                    new Vector3(i, j+1, 0),
                    new Vector3(i+1, j+1, 0)
                });
                triangles.AddRange(new int[]
                {
                    numberOfVertices + 0, numberOfVertices + 2, numberOfVertices + 1,
                    numberOfVertices + 2, numberOfVertices + 3, numberOfVertices + 1
                });
                normals.AddRange(new Vector3[]{Vector3.up, Vector3.up, Vector3.up, Vector3.up});
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        _meshFilter.mesh = mesh;
    }


}