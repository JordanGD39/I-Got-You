using System;
using System.Collections;
using System.Collections.Generic;
using TriangleNet.Geometry;
using UnityEngine;

public class PrimMinimumSpanningTree : MonoBehaviour
{
    private List<Edge> allEdges = new List<Edge>();
    [SerializeField] private List<Vector3> allPoints = new List<Vector3>();
    [SerializeField] private List<PrimEdgeData> primEdgeDatas = new List<PrimEdgeData>();

    public void CreateMinimumSpanningTree(TriangleNet.Mesh mesh)
    {
        foreach (Edge edge in mesh.Edges)
        {
            allEdges.Add(edge);
        }

        CreateEdgeDataList(mesh);

        //FindNeighbours();
    }

    private void CreateEdgeDataList(TriangleNet.Mesh mesh)
    {
        foreach (Edge edge in allEdges)
        {
            Vector3[] points = GetEdgePoints(mesh, edge);

            foreach (Vector3 point in points)
            {
                if (!allPoints.Contains(point))
                {
                    allPoints.Add(point);
                }
            }            
        }

        foreach (Edge edge in allEdges)
        {
            Vector3[] points = GetEdgePoints(mesh, edge);

            PrimEdgeData primEdgeData = new PrimEdgeData();
            primEdgeData.startNode = allPoints.IndexOf(points[0]);
            primEdgeData.endNode = allPoints.IndexOf(points[1]);
            primEdgeData.edgeCost = Vector3.Distance(points[0], points[1]);

            primEdgeDatas.Add(primEdgeData);
        }
    }

    private Vector3[] GetEdgePoints(TriangleNet.Mesh mesh, Edge edge)
    {
        Vertex v0 = mesh.vertices[edge.P0];
        Vertex v1 = mesh.vertices[edge.P1];
        Vector3 p0 = new Vector3((float)v0.x, 0.0f, (float)v0.y);
        Vector3 p1 = new Vector3((float)v1.x, 0.0f, (float)v1.y);

        Vector3[] points = { p0, p1 };

        return points;
    }
}

[Serializable]
public class PrimEdgeData
{
    public int startNode = 0;
    public int endNode = 0;
    public float edgeCost = 0;
}
