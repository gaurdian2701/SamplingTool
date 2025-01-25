using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public class PoissonDiskSampler
{
    private ProceduralGenerator _proceduralGenerator;
    private float _cellSize;

    public PoissonDiskSampler(ProceduralGenerator proceduralGenerator, float cellSize)
    {
        _proceduralGenerator = proceduralGenerator;
        _cellSize = cellSize;
    }
    public List<Vector2> GetPoissonDistribution()
    {
        List<Vector2> result = new List<Vector2>();
        List<Vector2> activePoints = new List<Vector2>();
        
        //Get Starting Point on Mesh
        int row = Random.Range(0, _proceduralGenerator.Resolution);
        int column = Random.Range(0, _proceduralGenerator.Resolution);
        
        MeshPoint randomMeshPoint = _proceduralGenerator.MeshPointData[row, column];
        Vector2 randomPointWithinMeshRectangle = new Vector2(randomMeshPoint.X + Random.Range(0, _proceduralGenerator.SamplingRadius),
            randomMeshPoint.Z + Random.Range(0, _proceduralGenerator.SamplingRadius));
        
        //Add starting point to lists
        result.Add(randomPointWithinMeshRectangle);
        activePoints.Add(randomPointWithinMeshRectangle);
        _proceduralGenerator.PoissonData[row, column] = randomMeshPoint;
        
        //Start populating from random point
        while (activePoints.Count > 0)
        {
            int randomPointIndex = Random.Range(0, activePoints.Count);
            bool foundValidPoint = false;
            Vector2 randomPoint = activePoints[randomPointIndex];
            //Find valid point
            for (int i = 0; i < _proceduralGenerator.SampleLimit; i++)
            {
                int randomDirection = Random.Range(0, 360);
                float randomDistanceFromPoint = Random.Range(_proceduralGenerator.SamplingRadius,
                    _proceduralGenerator.SamplingRadius * 2);
                Vector2 offsetVector = new Vector2(randomDistanceFromPoint + Mathf.Cos(randomDirection * Mathf.Deg2Rad), 
                    randomDistanceFromPoint + Mathf.Sin(randomDirection * Mathf.Deg2Rad));
                Vector2 finalPoint = randomPoint + offsetVector;

                if (IsValidPoint(finalPoint))
                {
                    Debug.Log("found valid point: " + finalPoint);
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.position = new Vector3(finalPoint.x, 0, finalPoint.y);
                    cube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                    foundValidPoint = true;
                    activePoints.Add(finalPoint);
                    result.Add(finalPoint);
                }
            }
            
            if(!foundValidPoint)
                activePoints.RemoveAt(randomPointIndex);
        }
        return result;
    }

    private bool IsValidPoint(Vector2 point)
    {
        int xindex = Mathf.FloorToInt(point.x / _cellSize);
        int yindex = Mathf.FloorToInt(point.y / _cellSize);
        
        Debug.Log("Point X: " + point.x + " Point Z: " + point.y + " X Index: " + xindex + " Y Index: " + yindex);

        if (xindex <= 0 || yindex <= 0 || xindex >= _proceduralGenerator.Resolution - 1||
            yindex >= _proceduralGenerator.Resolution - 1)
            return false;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                    continue;
                MeshPoint neighbourPoint = _proceduralGenerator.MeshPointData[xindex+i, yindex+j]; 
                if (neighbourPoint != null && !neighbourPoint.IsSampledPoint)
                {
                    Debug.Log("neighbour point: " + neighbourPoint.X + ", " + neighbourPoint.Z);
                    neighbourPoint.IsSampledPoint = true;
                    float neighbourDistance = Vector2.Distance(new Vector2(neighbourPoint.X, neighbourPoint.Z),
                        new Vector2(point.x, point.y));
                    if(neighbourDistance >= _proceduralGenerator.SamplingRadius && neighbourDistance <= _proceduralGenerator.SamplingRadius * 2)
                        return true;
                }
            }
        }
        return false;
    }
}
