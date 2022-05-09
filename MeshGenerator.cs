using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, int levelOfDetail, MeshSettings settings)
    {
        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int verticesPerLine = settings.verticesPerLine;
        Vector2 topLeft = new Vector2(-1, 1) * settings.meshChunkSize / 2f;
        MeshData meshData = new MeshData(verticesPerLine, meshSimplificationIncrement);

        //vertexIndicesMap håller koll på om relevant vertex ska vara en del av det slutgiltiga meshet.
        int[,] vertexIndicesMap = new int[verticesPerLine, verticesPerLine];
        int meshVertexIndex = 0;
        int outOfMeshVertexIndex = -1;

        for (int y = 0; y < verticesPerLine; y++)
        {
            for (int x = 0; x < verticesPerLine; x++)
            {
                bool isOutOfMeshVertex = y == 0 || y == verticesPerLine - 1 || x == 0 || x == verticesPerLine - 1;
                bool isSkippedVertex = x > 2 && x < verticesPerLine - 3 && y > 2
                    && y < verticesPerLine - 3 && ((x - 2) % meshSimplificationIncrement != 0
                    || (y - 2) % meshSimplificationIncrement != 0);

                if (isOutOfMeshVertex)
                {
                    vertexIndicesMap[x, y] = outOfMeshVertexIndex;
                    outOfMeshVertexIndex--;
                }
                else if (!isSkippedVertex)
                {
                    vertexIndicesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        for (int y = 0; y < verticesPerLine; y++)
        {
            for (int x = 0; x < verticesPerLine; x++)
            {
                bool isSkippedVertex = x > 2 && x < verticesPerLine - 3 && y > 2
                    && y < verticesPerLine - 3 && ((x - 2) % meshSimplificationIncrement != 0
                    || (y - 2) % meshSimplificationIncrement != 0);

                if (!isSkippedVertex)
                {
                    bool isOutOfMeshVertex = y == 0 || y == verticesPerLine - 1 || x == 0 || x == verticesPerLine - 1;
                    bool isEdgeVertex = (y == 1 || x == 1 || y == verticesPerLine - 2 || x == verticesPerLine - 2) && !isOutOfMeshVertex;
                    bool isMainVertex = (x - 2) % meshSimplificationIncrement == 0 && (y - 2) % meshSimplificationIncrement == 0 && !isOutOfMeshVertex && !isEdgeVertex;
                    bool isEdgeConnectionVertex = (y == 2 || y == verticesPerLine - 3 || x == 2 || x == verticesPerLine - 3) && !isOutOfMeshVertex && !isEdgeVertex && !isMainVertex;

                    int vertexIndex = vertexIndicesMap[x, y];
                    Vector2 percent = new Vector2(x - 1, y - 1) / (verticesPerLine - 3);
                    Vector2 vertexPosition2D = topLeft + new Vector2(percent.x, -percent.y) * settings.meshChunkSize;
                    float height = heightMap[x, y];

                    if(isEdgeConnectionVertex){
                        bool isVertical = x == 2 || x == verticesPerLine -3;
                        int distanceToMainVertexA = ((isVertical)?y - 2: x - 2) % meshSimplificationIncrement;
                        int distanceToMainVertexB = meshSimplificationIncrement - distanceToMainVertexA;
                        float distanceAToBPercent = distanceToMainVertexA/(float)meshSimplificationIncrement;

                        float mainVertexAHeight = heightMap[(isVertical)?x: x - distanceToMainVertexA, (isVertical)?y-distanceToMainVertexA:y];
                        float mainVertexBHeight = heightMap[(isVertical)?x: x + distanceToMainVertexB, (isVertical)?y+distanceToMainVertexB:y];

                        height = mainVertexAHeight * (1-distanceAToBPercent) + mainVertexBHeight * distanceAToBPercent;
                    }

                    meshData.AddVertex(new Vector3(vertexPosition2D.x, height, vertexPosition2D.y), percent, vertexIndex);
                    bool createTriangle = x < verticesPerLine - 1 && y < verticesPerLine - 1 && (!isEdgeConnectionVertex || (x != 2 && y != 2));

                    if (createTriangle)
                    {
                        int currentIncrement = (isMainVertex && x != verticesPerLine - 3 && y != verticesPerLine - 3) ? meshSimplificationIncrement : 1;

                        int a = vertexIndicesMap[x, y];
                        int b = vertexIndicesMap[x + currentIncrement, y];
                        int c = vertexIndicesMap[x, y + currentIncrement];
                        int d = vertexIndicesMap[x + currentIncrement, y + currentIncrement];
                        meshData.AddTriangle(a, d, c);
                        meshData.AddTriangle(d, a, b);
                    }
                    vertexIndex++;
                }
            }
        }

        meshData.BakeNormals();

        return meshData;
    }
}

public class MeshData
{
    Vector3[] vertices;
    Vector3[] outOfMeshVertices;
    Vector3[] bakedNormals;
    int[] triangles;
    int[] outOfMeshTriangles;
    Vector2[] uvs;

    int triangleIndex;
    int outOfMeshTriangleIndex;

    public MeshData(int verticesPerLine, int meshSimplificationIncrement)
    {
        int meshEdgeVertices = (verticesPerLine - 2) * 4 - 4;
        int edgeConnectionVertices = (meshSimplificationIncrement-1) * (verticesPerLine-5)/meshSimplificationIncrement*4;
        int mainVerticesPerLine = (verticesPerLine-5)/meshSimplificationIncrement +1;
        int mainVerticesTotal = mainVerticesPerLine*mainVerticesPerLine;

        vertices = new Vector3[meshEdgeVertices + edgeConnectionVertices + mainVerticesTotal];
        uvs = new Vector2[vertices.Length];

        int meshEdgeTriangles = ((verticesPerLine-3) * 4 - 4) * 2;
        int mainTriangles = (mainVerticesPerLine-1) * (mainVerticesPerLine-1) * 2;  
        triangles = new int[(meshEdgeTriangles + mainTriangles) * 3];

        outOfMeshVertices = new Vector3[verticesPerLine * 4 - 4];
        outOfMeshTriangles = new int[((verticesPerLine - 1) * 4 - 4) * 2 * 3];
    }

    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
    {
        //om vertexindex är mindre än 0 betyder detta att detta är en bordervertex som inte skall vara med i slutgiltiga meshet.
        //bordervertex genereras för att chunksen ska aligna korrekt
        if (vertexIndex < 0)
        {
            outOfMeshVertices[(-vertexIndex) - 1] = vertexPosition;

        }
        else
        {
            vertices[vertexIndex] = vertexPosition;
            uvs[vertexIndex] = uv;
        }
    }

    public void AddTriangle(int a, int b, int c)
    {
        if (a < 0 || b < 0 || c < 0)
        {
            outOfMeshTriangles[outOfMeshTriangleIndex] = a;
            outOfMeshTriangles[outOfMeshTriangleIndex + 1] = b;
            outOfMeshTriangles[outOfMeshTriangleIndex + 2] = c;
            outOfMeshTriangleIndex += 3;
        }
        else
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = bakedNormals;
        return mesh;
    }

    //Hemmasnickrad version av mesh.RecalculateNormals() för att undvika att gränserna mellan olika chunks är synliga
    Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalsFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        int borderTriangleCount = outOfMeshTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = outOfMeshTriangles[normalTriangleIndex];
            int vertexIndexB = outOfMeshTriangles[normalTriangleIndex + 1];
            int vertexIndexC = outOfMeshTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalsFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if (vertexIndexA >= 0)
            {
                vertexNormals[vertexIndexA] += triangleNormal;
            }
            if (vertexIndexB >= 0)
            {
                vertexNormals[vertexIndexB] += triangleNormal;
            }
            if (vertexIndexC >= 0)
            {
                vertexNormals[vertexIndexC] += triangleNormal;
            }
        }


        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    public void BakeNormals()
    {
        bakedNormals = CalculateNormals();
    }

    Vector3 SurfaceNormalsFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = (indexA < 0) ? outOfMeshVertices[(-indexA) - 1] : vertices[indexA];
        Vector3 pointB = (indexB < 0) ? outOfMeshVertices[(-indexB) - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? outOfMeshVertices[(-indexC) - 1] : vertices[indexC];

        //https://en.wikipedia.org/wiki/Cross_product
        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;

        return Vector3.Cross(sideAB, sideAC).normalized;
    }

}
