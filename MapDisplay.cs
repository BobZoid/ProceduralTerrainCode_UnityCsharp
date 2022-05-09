using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public DrawMode drawMode;
    public MeshSettings meshSettings;
    public HeightMapSettings mapSettings;
    public TextureSettings textureSettings;
    public Material mapMaterial;

    [Range(0, MeshSettings.amountOfDetailLevelsSupported-1)]
    public int editorPreviewDetailLevel;
    public bool autoUpdate;
    
    public void DrawTexture(Texture2D texture){
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3 (texture.width, 1, texture.height)/10f;
        textureRenderer.gameObject.SetActive(true);
        meshFilter.gameObject.SetActive(false);
    }

    public void DrawMesh(MeshData meshData){
        meshFilter.sharedMesh = meshData.CreateMesh();
        textureRenderer.gameObject.SetActive(false);
        meshFilter.gameObject.SetActive(true);
    }
    void OnValuesUpdated() {
		if (!Application.isPlaying) {
			DrawMapInEditor ();
		}
	}

    void OnTextureValuesUpdated() {
		textureSettings.ApplyToMaterial (mapMaterial);
	}

    public void DrawMapInEditor()
    {
        textureSettings.ApplyToMaterial(mapMaterial);
        //textureSettings.UpdateMeshHeights (mapMaterial, mapSettings.highest, mapSettings.lowest);
        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.verticesPerLine
        , meshSettings.verticesPerLine, mapSettings, Vector2.zero);

        if (drawMode == DrawMode.NoiseMap) {
			DrawTexture (TextureGenerator.TextureFromHeightMap (heightMap));
		} else if (drawMode == DrawMode.Mesh) {
			DrawMesh (MeshGenerator.GenerateTerrainMesh(heightMap.values, editorPreviewDetailLevel, meshSettings));
		} else if (drawMode == DrawMode.FalloffMap) {
			DrawTexture(TextureGenerator.TextureFromHeightMap((new HeightMap(FallofGenerator.GenerateFalloffMap(meshSettings.verticesPerLine), 0, 1))));
		}
    }

    private void OnValidate()
    {

       if (meshSettings != null) {
			meshSettings.OnValuesUpdated -= OnValuesUpdated;
			meshSettings.OnValuesUpdated += OnValuesUpdated;
		}
		if (mapSettings != null) {
			mapSettings.OnValuesUpdated -= OnValuesUpdated;
			mapSettings.OnValuesUpdated += OnValuesUpdated;
		}
		if (textureSettings != null) {
			textureSettings.OnValuesUpdated -= OnTextureValuesUpdated;
			textureSettings.OnValuesUpdated += OnTextureValuesUpdated;
		}
    }
}

public enum DrawMode
{
    NoiseMap,
    Mesh,
    FalloffMap
}