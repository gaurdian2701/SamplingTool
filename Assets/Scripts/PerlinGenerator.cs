using TreeEditor;
using UnityEngine;

public class PerlinGenerator
{
    private float _perlinNoiseScale;
    private float _persistence;
    private float _lacunarity;
    private float _heightModifier;
    private float _octaves;
    
    private float _minNoiseValue = float.MaxValue;
    private float _maxNoiseValue = float.MinValue;

    public PerlinGenerator(ProceduralGenerator proceduralGeneratorData)
    {
        _perlinNoiseScale = proceduralGeneratorData.PerlinNoiseScale;
        _persistence = proceduralGeneratorData.Persistence;
        _lacunarity = proceduralGeneratorData.Lacunarity;
        _heightModifier = proceduralGeneratorData.HeightModifier;
        _octaves = proceduralGeneratorData.Octaves;
    }
    public float GeneratePerlinData(int x, int y)
    {
        float frequency = 1;
        float amplitude = 1;
        float noiseHeight = 0f;

        for (int currentOctave = 0; currentOctave < _octaves; currentOctave++)
        {
            float sampleX = x/_perlinNoiseScale * frequency;
            float sampleY = y/_perlinNoiseScale * frequency;
            float perlinSample = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
            noiseHeight += perlinSample * amplitude * _heightModifier;
            
            if(noiseHeight < _minNoiseValue)
                _minNoiseValue = noiseHeight;
            if(noiseHeight > _maxNoiseValue)
                _maxNoiseValue = noiseHeight;
            frequency *= _lacunarity;
            amplitude *= _persistence;
        }
        
        return noiseHeight;
    }
}
