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
    [Range(0, 15)] public float PerlinNoiseScale;
    [Range(0, 1)] public float Lacunarity; //AFFECTS THE FREQUENCY OF AN OCTAVE
    [Range(0, 5)] public float Persistence; //AFFECTS THE DECREASE IN AMPLITUDE OF AN OCTAVE
    [Range(1, 50)] public float HeightModifier;
    [Range(1, 10)] public int Octaves;

    public GameObject WaterPrefab;
    
    private MeshFilter _meshFilter;
    private Tile[,] _tileData;
    private float[,] _rockData;
    
    private float _minNoiseValue = float.MaxValue;
    private float _maxNoiseValue = float.MinValue;
    
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector3> normals = new List<Vector3>();

    public void DoCreateGrid()
    {
        _tileData = new Tile[Resolution, Resolution];
        _rockData = new float[Resolution, Resolution];
        
        vertices = new List<Vector3>();
        triangles = new List<int>();
        normals = new List<Vector3>();
        
        _meshFilter = GetComponent<MeshFilter>();
        
        GenerateTileData();
        GenerateTerrainData();
    }

    private void Awake()
    {
        _tileData = new Tile[Resolution, Resolution];
        _rockData = new float[Resolution, Resolution];
        
        vertices = new List<Vector3>();
        triangles = new List<int>();
        normals = new List<Vector3>();
        
        _meshFilter = GetComponent<MeshFilter>();
    }

    private void Start()
    {
        GenerateTileData();
        GenerateTerrainData();
        GenerateRockData();
    }

    private void GenerateTileData()
    {
        for (int i = 0; i < Resolution; i++)
        {
            for (int j = 0; j < Resolution; j++)
            {
                Tile currentTile = new Tile();
                currentTile.X = i;
                currentTile.Z = j;
                _tileData[i, j] = currentTile;
            }
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
                _rockData[i,j] = GeneratePerlinData(i, j);
                vertices.AddRange(new Vector3[]
                {
                    new Vector3(currentTile.X, _rockData[i, j], currentTile.Z), //bottom left 
                    new Vector3(currentTile.X + 1, GeneratePerlinData(i+1, j), currentTile.Z), //bottom right
                    new Vector3(currentTile.X, GeneratePerlinData(i, j+1), currentTile.Z + 1), //top left
                    new Vector3(currentTile.X + 1, GeneratePerlinData(i+1, j+1), currentTile.Z + 1) //top right
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

    private void GenerateRockData()
    {
        for (int i = 0; i < Resolution; i+=1)
        {
            for (int j = 0; j < Resolution; j+=1)
            {
                if (_rockData[i, j] - _minNoiseValue <= 2f)
                {
                    GameObject rock = Instantiate(WaterPrefab);
                    rock.transform.position = new Vector3(i, _rockData[i, j], j);
                    rock.transform.up = _meshFilter.mesh.normals[i * Resolution + j];
                }
            }
        }
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