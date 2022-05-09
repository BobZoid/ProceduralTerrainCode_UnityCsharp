using UnityEngine;

public static class Noise
{
    public enum NormalizationMode { Local, Global }
    //Local mode för att normalisera värdena för enstaka chunks eftersom genereringen blir snyggare. 
    //Global för att normalisera så att chunksen alignar korrekt med varandra.
    public static float[,] GenerateNoiseMap(int xValue, int yValue, NoiseSettings settings, Vector2 sampleCenter)
    {
        float[,] noiseMap = new float[xValue, yValue];
        float halfX = xValue / 2;
        float halfY = yValue / 2;
        float maxNoiseHeightPossible = 0;
        float amplitude = 1;
        float frequency = 1;
        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        System.Random rand = new System.Random(settings.seed);
        Vector2[] octavesOffset = new Vector2[settings.octaves];
        for (int i = 0; i < settings.octaves; i++)
        {
            float offsetX = rand.Next(-100000, 100000) + settings.offset.x + sampleCenter.x;
            float offsetY = rand.Next(-100000, 100000) - settings.offset.y - sampleCenter.y;

            octavesOffset[i] = new Vector2(offsetX, offsetY);
            maxNoiseHeightPossible += amplitude;
            amplitude *= settings.persistance;
        }

        for (int y = 0; y < yValue; y++)
        {
            for (int x = 0; x < xValue; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < settings.octaves; i++)
                {
                    float sampleY = (y - halfY + octavesOffset[i].y) / settings.scale * frequency;
                    float sampleX = (x - halfY + octavesOffset[i].x) / settings.scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= settings.persistance;
                    frequency *= settings.lacunarity;
                }

                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }

                if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;

                if(settings.normalizationMode==NormalizationMode.Global){

                }
            }
        }
        if (settings.normalizationMode == NormalizationMode.Local)
        {
            for (int y = 0; y < yValue; y++)
            {
                for (int x = 0; x < xValue; x++)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                } 
            }
        }

        return noiseMap;
    }


}

[System.Serializable]
public class NoiseSettings
{
    public Noise.NormalizationMode normalizationMode;
    public Vector2 offset;
    public float scale = 50;
    public int octaves = 6;
    [Range(0, 1)]
    public float persistance = 0.6f;
    public float lacunarity = 2;
    public int seed;

    public void ValidateValues()
    {
        scale = Mathf.Max(scale, 0.001f);
        octaves = Mathf.Max(octaves, 1);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistance = Mathf.Clamp01(persistance);
    }
}
