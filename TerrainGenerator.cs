using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    const float viewerMovementBeforeChunkUpdate = 25f;
    const float squaredViewerMovementBeforeChunkUpdate = viewerMovementBeforeChunkUpdate * viewerMovementBeforeChunkUpdate;
    float meshChunkSize;
    int visibleChunksInViewDistance;
    public int colliderDetailLevelIndex;
    public bool killZoneActivated=false;
    public int killZoneScale=1;
    

    public LevelOfDetailInfo[] detailLevels;
    public MeshSettings meshSettings;
    public HeightMapSettings mapSettings;
    public TextureSettings textureSettings;

    public Transform viewer;
    public Material mapMaterial;
    public GameObject killZone;
    Vector2 viewerPosition;
    Vector2 viewerPositionPreviously;

    Dictionary<Vector2, TerrainChunk> chunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> chunksVisible = new List<TerrainChunk>();

    private void Start()
    {
        meshSettings.killZoneActivated=killZoneActivated;
        
        if(meshSettings.killZoneActivated==true){
            killZone.SetActive(true);
            killZone.transform.localScale=new Vector3(killZoneScale,1,killZoneScale);
            killZone.transform.GetChild(0).transform.localScale=new Vector3(killZoneScale,1,killZoneScale);
            killZone.transform.GetChild(1).transform.localScale=new Vector3(killZoneScale,1,killZoneScale);
        } else {
            killZone.SetActive(false);
        }
        textureSettings.ApplyToMaterial(mapMaterial);
        float maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        meshChunkSize = meshSettings.meshChunkSize;
        visibleChunksInViewDistance = Mathf.RoundToInt(maxViewDistance / meshChunkSize);

        UpdateVisibleChunks();
    }

    private void Update()
    {
        
            viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if (viewerPosition != viewerPositionPreviously)
        {
            foreach (TerrainChunk chunk in chunksVisible)
            {
                chunk.UpdateCollisionMesh();
            }
        }

        if ((viewerPositionPreviously - viewerPosition).sqrMagnitude > squaredViewerMovementBeforeChunkUpdate)
        {
            viewerPositionPreviously = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks()
    {
        HashSet<Vector2> updatedChunkCoordinates = new HashSet<Vector2>();
        for (int i = chunksVisible.Count - 1; i >= 0; i--)
        {
            updatedChunkCoordinates.Add(chunksVisible[i].coordinates);
            chunksVisible[i].UpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshChunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshChunkSize);

        for (int yOffset = -visibleChunksInViewDistance; yOffset <= visibleChunksInViewDistance; yOffset++)
        {
            for (int xOffset = -visibleChunksInViewDistance; xOffset <= visibleChunksInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (!updatedChunkCoordinates.Contains(viewedChunkCoord))
                {
                    if (chunkDictionary.ContainsKey(viewedChunkCoord))
                    {
                        chunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    }
                    else
                    {
                        TerrainChunk chunk = new TerrainChunk(viewedChunkCoord, transform, viewer, mapMaterial, detailLevels, colliderDetailLevelIndex, mapSettings, meshSettings);
                        chunkDictionary.Add(viewedChunkCoord, chunk);
                        chunk.onChunkStatusChange += OnChunkStatusChange;
                        chunk.Load();
                    }
                }
            }
        }
    }

    void OnChunkStatusChange(TerrainChunk chunk, bool visible){
        if(visible){
            chunksVisible.Add(chunk);
        } else
        {
            chunksVisible.Remove(chunk);
        }
    }
}

[System.Serializable]
    public struct LevelOfDetailInfo
    {
        [Range(0, MeshSettings.amountOfDetailLevelsSupported - 1)]
        public int levelOfDetail;
        public float visibleDistanceThreshold;

        public float squaredVisibleDistanceThreshold
        {
            get
            {
                return visibleDistanceThreshold * visibleDistanceThreshold;
            }
        }
    }