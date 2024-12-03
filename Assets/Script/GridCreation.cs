using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = System.Random;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GridCreation : MonoBehaviour
{
    public int Width;
    public int Height;

    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    
    private Tile[,] _tileData;
    private List<Color> _colorData;

    private void Awake()
    {
        _tileData = new Tile[Width, Height];
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshFilter = GetComponent<MeshFilter>();
        GenerateTileData();
        GenerateMeshData();
        UpdateMeshColorData();
        StartCoroutine(DoGameOfLife());
    }

    private void GenerateTileData()
    {
        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                Tile currentTile = new Tile();
                currentTile.X = (UInt16)i;
                currentTile.Y = (UInt16)j;
                currentTile.isAlive = UnityEngine.Random.Range(0, 2) > 0;
                _tileData[i, j] = currentTile;
            }
        }
    }

    private void GenerateMeshData()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        _colorData = new List<Color>();

        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                int numberOfVertices = vertices.Count;
                Tile currentTile = _tileData[i, j];
                vertices.AddRange(new Vector3[]
                {
                    new Vector3(currentTile.X, currentTile.Y, 0),
                    new Vector3(currentTile.X + 1, currentTile.Y, 0),
                    new Vector3(currentTile.X, currentTile.Y + 1, 0),
                    new Vector3(currentTile.X + 1, currentTile.Y + 1, 0)
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

    private void UpdateMeshColorData()
    {
        _colorData.Clear();
        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                Color colorToBeAdded = _tileData[i, j].isAlive ? Color.black : Color.white;
                _colorData.AddRange(new Color[]
                {
                    colorToBeAdded, colorToBeAdded, colorToBeAdded, colorToBeAdded,
                });
            }
        }
        _meshFilter.mesh.colors = _colorData.ToArray();   
    }

    private IEnumerator DoGameOfLife()
    {
        while (true)
        {
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    Tile currentTile = _tileData[i, j];
                    int neighboursOfTile = CheckNeighbours(currentTile);
                    if (currentTile.isAlive)
                    {
                        if(neighboursOfTile > 3)
                            currentTile.isAlive = false;
                        else if(neighboursOfTile < 2)
                            currentTile.isAlive = false;
                        else if(neighboursOfTile == 2 || neighboursOfTile == 3)
                            currentTile.isAlive = true;
                    }
                    else
                    {
                        if (neighboursOfTile == 3)
                            currentTile.isAlive = true;
                    }
                }
            }
            UpdateMeshColorData();
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    private int CheckNeighbours(Tile tile)
    {
        int aliveNeighbors = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if((i == 0 && j == 0) || tile.X + i >= Height - 1 || tile.Y + j >= Width - 1 || tile.X + i <= 0 || tile.Y + j <= 0)
                    continue;
                if(_tileData[tile.X + i, tile.Y + j].isAlive)
                    aliveNeighbors++;
            }
        }
        return aliveNeighbors;
    }
}