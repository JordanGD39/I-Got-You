using System;
using System.Collections;
using System.Collections.Generic;
using TriangleNet.Geometry;
using UnityEngine;

public class PrimMinimumSpanningTree : MonoBehaviour
{
    private List<Edge> allEdges = new List<Edge>();
    [SerializeField] private float randomEdgeChance = 15;
    [SerializeField] private float chanceToAddOneEdge = 80;
    [SerializeField] private List<Vector3> allPoints = new List<Vector3>();
    [SerializeField] private List<PrimNode> primNodes = new List<PrimNode>();
    [SerializeField] private List<PrimEdge> primQueue = new List<PrimEdge>();
    [SerializeField] private List<int> visistedNodes = new List<int>();
    [SerializeField] private List<PrimEdge> minimumSpanningTree = new List<PrimEdge>();
    public List<Vector3> AllPoints { get { return allPoints; } }
    public List<PrimEdge> MinimumSpanningTree { get { return minimumSpanningTree; } }
    [SerializeField] private List<DebugConnection> debugConnections = new List<DebugConnection>();

    private bool doneCreating = false;
    private bool showMST = false;
    public bool ShowMST { get { return showMST; } }
    private TriangleNet.Mesh mesh;
    private int maxEdgeIndex = 0;
    private int maxDebugNodes = 0;

    private void IncreaseMaxEdgeIndex()
    {
        maxEdgeIndex++;

        if (maxEdgeIndex >= allEdges.Count - 1 && doneCreating && !showMST)
        {
            showMST = true;
        }
    }

    public void StartCreationOfMinimumSpanningTree(TriangleNet.Mesh aMesh)
    {
        mesh = aMesh;
        foreach (Edge edge in aMesh.Edges)
        {
            allEdges.Add(edge);
        }

        CreateEdgeDataList();
        CreateMinimumSpanningTree();

        AddRandomEdgesBack();

        InvokeRepeating(nameof(IncreaseMaxEdgeIndex), 0.5f, 0.5f);
    }

    private void CreateEdgeDataList()
    {
        foreach (Edge edge in allEdges)
        {
            Vector3[] points = GetEdgePoints(edge);

            foreach (Vector3 point in points)
            {
                if (!allPoints.Contains(point))
                {
                    allPoints.Add(point);
                }
            }            
        }

        for (int i = 0; i < allPoints.Count; i++)
        {
            Vector3 point = allPoints[i];

            foreach (Edge edge in allEdges)
            {
                Vector3[] points = GetEdgePoints(edge);

                Vector3 pointPos = Vector3.positiveInfinity;
                Vector3 neighbourPos = Vector3.positiveInfinity;

                if (points[0] == point)
                {
                    pointPos = point;
                    neighbourPos = points[1];
                }
                else if (points[1] == point)
                {
                    pointPos = point;
                    neighbourPos = points[0];
                }
                else
                {
                    continue;
                }

                PrimNode primEdgeData = null;
                bool nodeAlreadyAdded = false;

                foreach (PrimNode node in primNodes)
                {
                    if (node.startNode == i)
                    {
                        primEdgeData = node;
                        nodeAlreadyAdded = true;
                    }
                }

                if (primEdgeData == null)
                {
                    primEdgeData = new PrimNode();
                    primEdgeData.startNode = i;
                }

                primEdgeData.neighbours.Add(allPoints.IndexOf(neighbourPos));
                primEdgeData.edgesCost.Add(Vector3.Distance(point, neighbourPos));

                if (!nodeAlreadyAdded)
                {
                    primNodes.Add(primEdgeData);
                }
            }
        }
    }

    private Vector3[] GetEdgePoints(Edge edge)
    {
        Vertex v0 = mesh.vertices[edge.P0];
        Vertex v1 = mesh.vertices[edge.P1];
        Vector3 p0 = new Vector3((float)v0.x, 0.0f, (float)v0.y);
        Vector3 p1 = new Vector3((float)v1.x, 0.0f, (float)v1.y);

        Vector3[] points = { p0, p1 };

        return points;
    }

    private void CreateMinimumSpanningTree()
    {
        int loopCounter = 0, maxLoops = 999; //just an example
        int currentNode = 0;
        int currentSearchIndex = 0;
        visistedNodes.Add(0);

        while (visistedNodes.Count < primNodes.Count)
        {
            if (loopCounter >= maxLoops)
            {
                Debug.LogError("INFINTE WHILE LOOP DETECTED!");
                break;
            }

            if (currentNode >= 0)
            {
                for (int i = 0; i < primNodes[currentNode].neighbours.Count; i++)
                {
                    PrimEdge primEdge = new PrimEdge();

                    primEdge.startNode = primNodes[currentNode].startNode;
                    primEdge.endNode = primNodes[currentNode].neighbours[i];
                    primEdge.edgeCost = primNodes[currentNode].edgesCost[i];

                    primQueue.Add(primEdge);

                    Debug.Log("Add edge: " + primEdge.startNode + ", " + primEdge.endNode + ", " + primEdge.edgeCost + " to queue");
                }
            }            

            float lowestCost = Mathf.Infinity;
            PrimEdge promisingEdge = null;
            int newSearchIndex = -1;

            for (int i = currentSearchIndex; i < primQueue.Count; i++)
            {
                PrimEdge edge = primQueue[i];

                if (currentNode < 0)
                {
                    Debug.Log("Edge: " + edge.startNode + ", " + edge.endNode + ", " + edge.edgeCost + " is being checked!");
                }

                if (!visistedNodes.Contains(edge.endNode) && edge.edgeCost < lowestCost)
                {
                    lowestCost = edge.edgeCost;
                    promisingEdge = edge;
                    newSearchIndex = i;
                    Debug.Log("Edge: " + edge.startNode + ", " + edge.endNode + ", " + edge.edgeCost + " is most promising!");
                }
            }            

            if (promisingEdge == null)
            {
                Debug.Log("MST stuck!");
                currentSearchIndex = 0;
                currentNode = -1;

                loopCounter++;
                continue;
            }

            primQueue.Remove(promisingEdge);
            minimumSpanningTree.Add(promisingEdge);
            visistedNodes.Add(promisingEdge.endNode);
            currentSearchIndex = primQueue.Count;
            currentNode = promisingEdge.endNode;            
            
            Debug.Log("current search index is: " + currentSearchIndex);

            loopCounter++;
        }

        doneCreating = true;

        maxDebugNodes = minimumSpanningTree.Count;
    }

    private void AddRandomEdgesBack()
    {
        List<PrimEdge> dups = new List<PrimEdge>();

        foreach (PrimEdge edge in primQueue)
        {
            foreach (PrimEdge checkEdge in minimumSpanningTree)
            {
                if ((checkEdge.startNode == edge.endNode && checkEdge.endNode == edge.startNode) || 
                    (checkEdge.startNode == edge.startNode && checkEdge.endNode == edge.endNode))
                {
                    dups.Add(edge);
                }
            }
        }

        foreach (PrimEdge edge in dups)
        {
            primQueue.Remove(edge);
        }

        int addedEdges = 0;

        foreach (PrimEdge edge in primQueue)
        {
            float rand = UnityEngine.Random.Range(0, 100);

            if (rand < randomEdgeChance)
            {
                minimumSpanningTree.Add(edge);
                addedEdges++;
            }
        }

        if (addedEdges == 0)
        {
            float rand = UnityEngine.Random.Range(0, 100);

            if (rand < chanceToAddOneEdge)
            {
                int randIndex = UnityEngine.Random.Range(0, primQueue.Count);

                minimumSpanningTree.Add(primQueue[randIndex]);
            }
        }
    }

    public void OnDrawGizmosSelected()
    {
        if (!doneCreating)
        {
            // We're probably in the editor
            return;
        }

        if (showMST)
        {
            Gizmos.color = Color.green;

            foreach (PrimEdge edge in minimumSpanningTree)
            {
                Gizmos.DrawLine(allPoints[edge.startNode], allPoints[edge.endNode]);

                if (debugConnections.Count < minimumSpanningTree.Count)
                {
                    DebugConnection debugConnection = new DebugConnection();
                    debugConnection.startConnection = allPoints[edge.startNode];
                    debugConnection.endConnection = allPoints[edge.endNode];

                    debugConnections.Add(debugConnection);
                }                
            }
        }

        Gizmos.color = Color.gray;
        int index = 0;

        foreach (Edge edge in allEdges)
        {
            if (index > maxEdgeIndex)
            {
                break;
            }

            Vertex v0 = mesh.vertices[edge.P0];
            Vertex v1 = mesh.vertices[edge.P1];
            Vector3 p0 = new Vector3((float)v0.x, 0.0f, (float)v0.y);
            Vector3 p1 = new Vector3((float)v1.x, 0.0f, (float)v1.y);

            bool connectionAldreadyDrew = false;

            foreach (DebugConnection connection in debugConnections)
            {
                if ((p0 == connection.startConnection && p1 == connection.endConnection) || (p1 == connection.startConnection && p0 == connection.endConnection))
                {
                    connectionAldreadyDrew = true;
                    break;
                }
            }

            if (connectionAldreadyDrew)
            {
                continue;
            }

            Gizmos.DrawLine(p0, p1);

            index++;
        }
    }
}

[Serializable]
public class PrimNode
{
    public int startNode = 0;
    public List<int> neighbours = new List<int>();
    public List<float> edgesCost = new List<float>();
}

[Serializable]
public class PrimEdge
{
    public int startNode = 0;
    public int endNode = 0;
    public float edgeCost = 0;
}

[Serializable]
public class DebugConnection
{
    public Vector3 startConnection;
    public Vector3 endConnection;
}
