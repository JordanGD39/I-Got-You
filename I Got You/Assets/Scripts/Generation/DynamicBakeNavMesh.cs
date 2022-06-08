using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class DynamicBakeNavMesh : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            return;
        }

        DungeonGenerator dungeonGenerator = FindObjectOfType<DungeonGenerator>();

        if (dungeonGenerator.StartingRoom == null)
        {
            dungeonGenerator.OnGenerationDone += GetComponent<NavMeshSurface>().BuildNavMesh;
        }
        else
        {
            GetComponent<NavMeshSurface>().BuildNavMesh();
        }        
    }
}
