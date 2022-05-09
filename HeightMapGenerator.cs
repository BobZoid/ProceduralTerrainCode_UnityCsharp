using UnityEngine;

public static class HeightMapGenerator
{
    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCenter){
        float[,] values = Noise.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCenter);

        AnimationCurve threadsafeHeightCurve = new AnimationCurve(settings.heightCurve.keys);

        float lowest = float.MaxValue;
        float highest = float.MinValue;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                values[x,y] *= threadsafeHeightCurve.Evaluate(values[x,y]) * settings.heightMultiplier;

                if(values[x,y]>highest){
                    highest=values[x,y];
                }
                if(values[x,y]<lowest){
                    lowest=values[x,y];
                }
            }   
        }

        return new HeightMap(values, highest, lowest);
    }

}

public struct HeightMap
{
    public readonly float[,] values;
    public readonly float highestValue;
    public readonly float lowestValue;

    public HeightMap(float[,] values, float highValue, float lowValue)
    {
        this.values = values;
        highestValue = highValue;
        lowestValue = lowValue;
    }
}
