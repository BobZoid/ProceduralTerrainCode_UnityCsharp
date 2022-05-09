using UnityEngine;

[CreateAssetMenu()]
public class MeshSettings : Updateable {
	
	public float scale = 2f;
    public bool killZoneActivated;
    public const int amountOfDetailLevelsSupported=5;
	public const int amountOfChunkSizesSupported = 9;
	public static readonly int[] supportedChunkSizes = {48, 72, 96, 120, 144, 168, 192, 216, 240};
    [Range(0,amountOfChunkSizesSupported-1)]
    public int chunkSizeIndex;

    //Antalet punkter i ett mesh skapat med bästa (0) detaljnivå (LevelOfDetail) + två till som används för att räkna ut normalvärden
    public int verticesPerLine
    {
        get{
            return supportedChunkSizes[chunkSizeIndex] + 5;
        }
    }

    public float meshChunkSize{
        get{
            return (verticesPerLine-3) * scale;
        }
    }
	
}

