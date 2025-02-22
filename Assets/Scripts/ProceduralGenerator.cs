using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = System.Random;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class ProceduralGenerator : MonoBehaviour
{
    public PoissonDiskSampler PoissonDiskSampler;
    public MeshCollider TerrainMeshCollider;
    [Range(1, 80)]public int Resolution;
    [Range(1, 15)] public float PerlinNoiseScale;
    [Range(0, 1)] public float Lacunarity; //AFFECTS THE FREQUENCY OF AN OCTAVE
    [Range(1, 5)] public float Persistence; //AFFECTS THE DECREASE IN AMPLITUDE OF AN OCTAVE
    [Range(1, 5)] public float HeightModifier;
    [Range(1, 10)] public int Octaves;
    
    public MeshPoint[,] MeshPointData;
    
    private MeshFilter _meshFilter;
    private PerlinGenerator _perlinGenerator;
    
    private float _cellSize;
    
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector3> normals = new List<Vector3>();
    

    private void Awake()
    {
        InitializeMeshData();
    }

    private void Start()
    {
        GenerateMeshData();
        GenerateTerrainData();
    }
    
    private void OnValidate()
    {
        DoCreateGrid();
    }
    
    public void DoCreateGrid()
    {
        InitializeMeshData();
        _perlinGenerator = new PerlinGenerator(this);
        GenerateMeshData();
        GenerateTerrainData();
    }

    private void InitializeMeshData()
    {
        MeshPointData = new MeshPoint[Resolution, Resolution];
        
        vertices = new List<Vector3>();
        triangles = new List<int>();
        normals = new List<Vector3>();
        
        _meshFilter = GetComponent<MeshFilter>();
        _cellSize = PoissonDiskSampler.SamplingRadius / Mathf.Sqrt(2);
    }

    public void ResetMeshCollider()
    {
        TerrainMeshCollider = gameObject.AddComponent<MeshCollider>();
        TerrainMeshCollider.convex = true;
    }
    private void GenerateMeshData()
    {
        float xPos = 0f;
        float zPos = 0f;
        for (int i = 0; i < Resolution; i++)
        {
            for (int j = 0; j < Resolution; j++)
            {
                MeshPoint currentMeshPoint = new MeshPoint();
                currentMeshPoint.X = xPos;
                currentMeshPoint.Z = zPos;
                MeshPointData[i, j] = currentMeshPoint;
                zPos += _cellSize;
            }
            xPos += _cellSize;
            zPos = 0f;
        }
    }

    private void GenerateTerrainData()
    {
        GeneratePerlinMesh();

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        _meshFilter.mesh = mesh;
    }

    private void GeneratePerlinMesh()
    {
        for (int i = 0; i < Resolution; i++)
        {
            for (int j = 0; j < Resolution; j++)
            {
                int numberOfVertices = vertices.Count;
                MeshPoint currentMeshPoint = MeshPointData[i, j];
                vertices.AddRange(new Vector3[]
                {
                    new Vector3(currentMeshPoint.X, GeneratePerlinData(i, j), currentMeshPoint.Z), //bottom left 
                    new Vector3(currentMeshPoint.X + _cellSize, GeneratePerlinData(i+1, j), currentMeshPoint.Z), //bottom right
                    new Vector3(currentMeshPoint.X, GeneratePerlinData(i, j+1), currentMeshPoint.Z + _cellSize), //top left
                    new Vector3(currentMeshPoint.X + _cellSize, GeneratePerlinData(i+1, j+1), currentMeshPoint.Z + _cellSize) //top right
                });
                triangles.AddRange(new int[]
                {
                    numberOfVertices + 0, numberOfVertices + 2, numberOfVertices + 1,
                    numberOfVertices + 2, numberOfVertices + 3, numberOfVertices + 1
                });
                normals.AddRange(new Vector3[]{Vector3.up, Vector3.up, Vector3.up, Vector3.up});
            }
        } 
    }

    private float GeneratePerlinData(int x, int y)
    {
        return _perlinGenerator.GeneratePerlinData(x, y);
    }

}