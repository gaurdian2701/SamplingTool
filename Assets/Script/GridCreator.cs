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
    [Range(1, 20)]public float PerlinNoiseScale;
    public int Octaves;
    public float Persistence;
    public float Lacunarity;
    
    private int _width;
    private  int _height;
    
    private MeshFilter _meshFilter;
    private Tile[,] _tileData;
    private float[,] _noiseMap;
    
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector3> normals = new List<Vector3>();

    public void DoCreateGrid()
    {
        _width = Resolution;
        _height = Resolution;
        
        _tileData = new Tile[_width, _height];
        _noiseMap = new float[_width, _height];
        vertices = new List<Vector3>();
        triangles = new List<int>();
        normals = new List<Vector3>();
        
        _meshFilter = GetComponent<MeshFilter>();
        
        GenerateTileData();
        GenerateMeshData();
    }

    private void GenerateTileData()
    {
        for (int i = 0; i < _height; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                Tile currentTile = new Tile();
                currentTile.X = i;
                currentTile.Y = j;
                currentTile.isAlive = UnityEngine.Random.Range(0, 2) > 0;
                _tileData[i, j] = currentTile;
            }
        }
    }

    private void GenerateMeshData()
    {
        for (int i = 0; i < _height; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                int numberOfVertices = vertices.Count;
                Tile currentTile = _tileData[i, j];
                vertices.AddRange(new Vector3[]
                {
                    new Vector3(currentTile.X, GeneratePerlinData(i,j), currentTile.Y), //bottom left 
                    new Vector3(currentTile.X + 1, GeneratePerlinData(i+1, j), currentTile.Y), //bottom right
                    new Vector3(currentTile.X, GeneratePerlinData(i, j+1), currentTile.Y + 1), //top left
                    new Vector3(currentTile.X + 1, GeneratePerlinData(i+1, j+1), currentTile.Y + 1) //top right
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
        float _noiseHeight = 0f;

        for (int k = 0; k < Octaves; k++)
        {
            float sample_x = x/PerlinNoiseScale * frequency;
            float sample_y = y/PerlinNoiseScale * frequency;
            float perlinSample = Mathf.PerlinNoise(sample_x, sample_y) * 2 - 1;
            _noiseHeight += perlinSample * amplitude;
            frequency *= Lacunarity;
            amplitude *= Persistence;
        }
        
        return _noiseHeight;
    }
}