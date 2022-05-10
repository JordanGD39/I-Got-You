using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PuzzleEat : MonoBehaviourPun
{
    [SerializeField] private List<LootObject> lootObjects = new List<LootObject>();
    //[SerializeField] private List<>
    private PlayerManager playerManager;

    public void StartPuzzle()
    {
        LootRoomGenerator lootRoomGenerator = GetComponent<LootRoomGenerator>();

        lootObjects = lootRoomGenerator.AllLoot;
        AddKeycards();
    }

    private void AddKeycards()
    {
        playerManager = FindObjectOfType<PlayerManager>();

        int playerCount = playerManager.Players.Count;

        for (int i = 0; i < playerCount; i++)
        {
            int rand = Random.Range(0, lootObjects.Count);
            PlaceKeycard(rand);
        }
    }

    private void PlaceKeycard(int index)
    {
        //lootObjects
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
