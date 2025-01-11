using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = System.Random;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GridCreator : MonoBehaviour
{
    [Range(1, 100)] public int Resolution;
    [Range(1, 10)] public float SamplingRadius;
    [Range(1, 15)] public float PerlinNoiseScale;
    [Range(0, 1)] public float Lacunarity; //AFFECTS THE FREQUENCY OF AN OCTAVE
    [Range(1, 5)] public float Persistence; //AFFECTS THE DECREASE IN AMPLITUDE OF AN OCTAVE
    [Range(1, 5)] public float HeightModifier;
    [Range(1, 10)] public int Octaves;
    
    private MeshFilter _meshFilter;
    private Tile[,] _tileData;
    
    private float _minNoiseValue = float.MaxValue;
    private float _maxNoiseValue = float.MinValue;
    private float _distanceBetweenVertices;
    
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector3> normals = new List<Vector3>();
    

    private void Awake()
    {
        InitializeTileData();
    }

    private void Start()
    {
        GenerateTileData();
        GenerateTerrainData();
    }
    
    private void OnValidate()
    {
        DoCreateGrid();
    }
    
    public void DoCreateGrid()
    {
        InitializeTileData();
        GenerateTileData();
        GenerateTerrainData();
    }

    private void InitializeTileData()
    {
        _tileData = new Tile[Resolution, Resolution];
        
        vertices = new List<Vector3>();
        triangles = new List<int>();
        normals = new List<Vector3>();
        
        _meshFilter = GetComponent<MeshFilter>();
        _distanceBetweenVertices = SamplingRadius / Mathf.Sqrt(2);
    }
    private void GenerateTileData()
    {
        float xPos = 0f;
        float zPos = 0f;
        for (int i = 0; i < Resolution; i++)
        {
            for (int j = 0; j < Resolution; j++)
            {
                Tile currentTile = new Tile();
                currentTile.X = xPos;
                currentTile.Z = zPos;
                _tileData[i, j] = currentTile;
                zPos += _distanceBetweenVertices;
            }
            xPos += _distanceBetweenVertices;
            zPos = 0f;
        }
    }

    private void GenerateTerrainData()
    {
        for (int i = 0; i < Resolution; i++)
        {
            for (int j = 0; j < Resolution; j++)
            {
                int numberOfVertices = vertices.Count;
                Tile currentTile = _tileData[i, j];
                vertices.AddRange(new Vector3[]
                {
                    new Vector3(currentTile.X, GeneratePerlinData(i, j), currentTile.Z), //bottom left 
                    new Vector3(currentTile.X + _distanceBetweenVertices, GeneratePerlinData(i+1, j), currentTile.Z), //bottom right
                    new Vector3(currentTile.X, GeneratePerlinData(i, j+1), currentTile.Z + _distanceBetweenVertices), //top left
                    new Vector3(currentTile.X + _distanceBetweenVertices, GeneratePerlinData(i+1, j+1), currentTile.Z + _distanceBetweenVertices) //top right
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
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        _meshFilter.mesh = mesh;
    }
    private float GeneratePerlinData(int x, int y)
    {
        float frequency = 1;
        float amplitude = 1;
        float noiseHeight = 0f;

        for (int currentOctave = 0; currentOctave < Octaves; currentOctave++)
        {
            float sample_x = x/PerlinNoiseScale * frequency;
            float sample_y = y/PerlinNoiseScale * frequency;
            float perlinSample = Mathf.PerlinNoise(sample_x, sample_y) * 2 - 1;
            noiseHeight += perlinSample * amplitude * HeightModifier;
            
            if(noiseHeight < _minNoiseValue)
                _minNoiseValue = noiseHeight;
            if(noiseHeight > _maxNoiseValue)
                _maxNoiseValue = noiseHeight;
            frequency *= Lacunarity;
            amplitude *= Persistence;
        }
        
        return noiseHeight;
    }
}