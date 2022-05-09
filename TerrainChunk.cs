using UnityEngine;


public class TerrainChunk
{
    public event System.Action<TerrainChunk, bool> onChunkStatusChange;
    public Vector2 coordinates;
    Vector2 sampleCenter;
    GameObject meshObject;
    Bounds bounds;
    MeshRenderer renderer;
    MeshFilter filter;
    MeshCollider collider;
    LevelOfDetailInfo[] detailLevels;
    LevelOfDetailMesh[] meshDetailLevels;

    const float generateCollidersWithinDistance = 5;
    int colliderDetailLevelIndex;
    int previousLevelOfDetailIndex = -1;
    float maxViewDistance;
    HeightMap heightMap;
    bool mapDataReceived;
    bool colliderSet;

    HeightMapSettings mapSettings;
    MeshSettings meshSettings;
    Transform viewer;
    Vector2 viewerPosition
    {
        get
        {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }

    public TerrainChunk(Vector2 coordinates, Transform parent, Transform viewer, Material material, LevelOfDetailInfo[] detailLevels
    , int colliderDetailLevelIndex, HeightMapSettings mapSettings, MeshSettings meshSettings)
    {
        this.coordinates = coordinates;
        this.detailLevels = detailLevels;
        this.colliderDetailLevelIndex = colliderDetailLevelIndex;
        this.mapSettings = mapSettings;
        this.meshSettings = meshSettings;
        this.viewer = viewer;

        sampleCenter = coordinates * meshSettings.meshChunkSize / meshSettings.scale;
        Vector2 position = coordinates * meshSettings.meshChunkSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.meshChunkSize);

        meshObject = new GameObject("Ground");
        meshObject.tag = "Ground";
        meshObject.layer = 6;
        renderer = meshObject.AddComponent<MeshRenderer>();
        filter = meshObject.AddComponent<MeshFilter>();
        collider = meshObject.AddComponent<MeshCollider>();
        renderer.material = material;

        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;
        SetVisible(false);

        meshDetailLevels = new LevelOfDetailMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++)
        {
            meshDetailLevels[i] = new LevelOfDetailMesh(detailLevels[i].levelOfDetail);
            meshDetailLevels[i].updateCallback += UpdateTerrainChunk;
            if (i == colliderDetailLevelIndex)
            {
                meshDetailLevels[i].updateCallback += UpdateCollisionMesh;
            }
        }

        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
    }


    public void Load()
    {
        ThreadSpinner.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.verticesPerLine
        , meshSettings.verticesPerLine, mapSettings, sampleCenter), OnHeightMapReceived);
    }

    public void UpdateTerrainChunk()
    {
        if (mapDataReceived)
        {
            float viewerDistanceFromClosestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool wasVisible = IsVisible();
            bool visible = viewerDistanceFromClosestEdge <= maxViewDistance;

            if (visible)
            {
                int detailLevelIndex = 0;

                for (int i = 0; i < detailLevels.Length - 1; i++)
                {
                    if (viewerDistanceFromClosestEdge > detailLevels[i].visibleDistanceThreshold)
                    {
                        detailLevelIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                if (detailLevelIndex != previousLevelOfDetailIndex)
                {
                    LevelOfDetailMesh meshDetailLevel = meshDetailLevels[detailLevelIndex];
                    if (meshDetailLevel.hasReceivedMesh)
                    {
                        previousLevelOfDetailIndex = detailLevelIndex;
                        filter.mesh = meshDetailLevel.mesh;
                    }
                    else if (!meshDetailLevel.hasRequestedMesh)
                    {
                        meshDetailLevel.RequestMesh(heightMap, meshSettings);
                    }
                }
            }
            if (wasVisible != visible)
            {
                SetVisible(visible);
                if (onChunkStatusChange != null)
                {
                    onChunkStatusChange(this, visible);
                }
                else
                {

                }
            }
        }
    }

    public void UpdateCollisionMesh()
    {
        if (colliderSet == false)
        {
            float squaredViewerToChunkDistance = bounds.SqrDistance(viewerPosition);

            if (squaredViewerToChunkDistance < detailLevels[colliderDetailLevelIndex]
            .squaredVisibleDistanceThreshold)
            {
                if (meshDetailLevels[colliderDetailLevelIndex].hasRequestedMesh == false)
                {
                    meshDetailLevels[colliderDetailLevelIndex].RequestMesh(heightMap, meshSettings);
                }
            }

            if(squaredViewerToChunkDistance < generateCollidersWithinDistance * generateCollidersWithinDistance){
                if (meshDetailLevels[colliderDetailLevelIndex].hasReceivedMesh)
                {
                    collider.sharedMesh = meshDetailLevels[colliderDetailLevelIndex].mesh;
                    colliderSet = true;
                }
            }
        }
    }

    void OnHeightMapReceived(object heightMap)
    {
        this.heightMap = (HeightMap)heightMap;
        mapDataReceived = true;

        UpdateTerrainChunk();
    }

    public void SetVisible(bool visible)
    {
        meshObject.SetActive(visible);
        //Kan bli problematiskt om för många chunks har skapats att de inte raderas
    }

    public bool IsVisible()
    {
        return meshObject.activeSelf;
    }

}

class LevelOfDetailMesh
{
    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasReceivedMesh;
    public event System.Action updateCallback;
    int levelOfDetail;

    public LevelOfDetailMesh(int levelOfDetail)
    {
        this.levelOfDetail = levelOfDetail;
    }

    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        hasRequestedMesh = true;
        ThreadSpinner.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, levelOfDetail, meshSettings), OnMeshDataReceived);
    }

    void OnMeshDataReceived(object meshData)
    {
        mesh = ((MeshData)meshData).CreateMesh();
        hasReceivedMesh = true;
        updateCallback();
    }

}