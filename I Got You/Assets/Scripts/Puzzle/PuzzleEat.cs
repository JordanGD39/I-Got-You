using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PuzzleEat : MonoBehaviourPun
{
    private LootRoomGenerator lootRoomGenerator;
    [SerializeField] private List<LootObject> lootObjects = new List<LootObject>();
    [SerializeField] private GameObject keycardPrefab;
    [SerializeField] private List<KeycardObject> availableKeyCardsPool = new List<KeycardObject>();
    [SerializeField] private List<KeycardObject> usedKeycardsPool = new List<KeycardObject>();
    [SerializeField] private List<PlayerStats.ClassNames> keycardRoles = new List<PlayerStats.ClassNames>();
    [SerializeField] private List<PlayerStats.ClassNames> availableKeycardRoles;
    private PlayerManager playerManager;
    [SerializeField] private DoorOpen controlRoomDoor;
    [SerializeField] private DoorOpen mazeDoor;
    [SerializeField] private DoorOpen[] doorsOut;

    private bool puzzleStarted = false;
    private int keycardsDelivered = 0;

    private void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();

        int playerCount = PhotonNetwork.IsConnected ? PhotonNetwork.CountOfPlayersInRooms : 1;

        if (playerCount <= 1)
        {
            playerCount = 2;
        }

        for (int i = 0; i < playerCount - 1; i++)
        {
            if (!PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected)
            {
                break;
            }

            GameObject keycard = PhotonFunctionHandler.InstantiateGameObject(keycardPrefab, Vector3.zero, Quaternion.identity);
            Debug.Log("Instantiating " + keycard.name);

            availableKeyCardsPool.Add(keycard.GetComponent<KeycardObject>());
            keycard.SetActive(false);

            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("AddKeycardToListOthers", RpcTarget.Others, keycard.GetPhotonView().ViewID);
            }
        }

        if (PhotonNetwork.IsConnected)
        {
            controlRoomDoor.OnOpenedDoor += StartPuzzle;
        }        
        else
        {
            mazeDoor.OnOpenedDoor += StartPuzzle;
            mazeDoor.PlayerOpen = true;
            controlRoomDoor.PlayerOpen = false;
            controlRoomDoor.OpenOnly = true;
        }
    }

    [PunRPC]
    void AddKeycardToListOthers(int viewId)
    {
        GameObject keycard = PhotonNetwork.GetPhotonView(viewId).gameObject;
        availableKeyCardsPool.Add(keycard.GetComponent<KeycardObject>());

        keycard.SetActive(false);
    }

    public void StartPuzzle()
    {
        if (puzzleStarted)
        {
            return;
        }

        puzzleStarted = true;

        if (!PhotonNetwork.IsConnected)
        {
            controlRoomDoor.OpenDoor();
            //controlRoomDoor.OpenClosedDoor();
        }

        lootRoomGenerator = GetComponent<LootRoomGenerator>();
        lootRoomGenerator.UpdateLootBasedOnPlayers();

        lootObjects = lootRoomGenerator.AllLoot;
        AddKeycards();
    }

    private void AddKeycardRoles(GameObject playerInControlRoom)
    {
        keycardRoles.Clear();

        foreach (PlayerStats player in playerManager.Players)
        {
            if (playerInControlRoom != player.gameObject)
            {
                keycardRoles.Add(player.CurrentClass);
            }
        }

        availableKeycardRoles = new List<PlayerStats.ClassNames>(keycardRoles);
    }

    [PunRPC]
    void AddKeycardRolesOthers(int viewId)
    {
        AddKeycardRoles(PhotonNetwork.GetPhotonView(viewId).gameObject);
        mazeDoor.OpenDoor();
    }

    private void AddKeycards()
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            return;
        }

        GameObject playerOpeningControlRoom = PhotonNetwork.IsConnected ? controlRoomDoor.PlayersInRange[0].transform.root.gameObject : null;

        AddKeycardRoles(playerOpeningControlRoom);

        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("AddKeycardRolesOthers", RpcTarget.Others, playerOpeningControlRoom.GetPhotonView().ViewID);
        }

        int playerCount = playerManager.Players.Count - 1;

        if (playerCount == 0)
        {
            playerCount = 1;
        }

        //Debug.Log(playerCount + " count");

        for (int i = 0; i < playerCount; i++)
        {
            int randomClassIndex = Random.Range(0, availableKeycardRoles.Count);
            int randomLootPositionIndex = Random.Range(0, lootObjects.Count);

            PlaceKeycard(randomClassIndex, randomLootPositionIndex);

            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("PlaceKeycardOthers", RpcTarget.Others, (byte)randomClassIndex, (byte)randomLootPositionIndex);
            }
        }

        mazeDoor.OpenDoor();
        lootRoomGenerator.PlaceLoot();
    }

    private void PlaceKeycard(int randomClassIndex, int randomLootPositionIndex)
    {
        //int rand = Random.Range(0, availableKeycardRoles.Count);

        //Debug.Log(randomClassIndex + " " + randomLootPositionIndex);

        PlayerStats.ClassNames classToGive = availableKeycardRoles[randomClassIndex];

        //rand = Random.Range(0, lootObjects.Count);
        LootObject lootObject = lootObjects[randomLootPositionIndex];
        Vector3 pos = lootObject.transform.position;

        KeycardObject keycard = availableKeyCardsPool[0];
        usedKeycardsPool.Add(keycard);
        availableKeyCardsPool.RemoveAt(0);

        keycard.AssignedClass = classToGive;
        keycard.transform.position = pos;
        keycard.gameObject.SetActive(true);

        lootObjects.RemoveAt(randomLootPositionIndex);
        availableKeycardRoles.RemoveAt(randomClassIndex);
    }

    [PunRPC]
    void PlaceKeycardOthers(byte randomClassIndex, byte randomLootPositionIndex)
    {
        PlaceKeycard(randomClassIndex, randomLootPositionIndex);
    }

    public void CheckPuzzleCompletion(bool localPlayerDelivered)
    {
        if (PhotonNetwork.IsConnected && localPlayerDelivered)
        {
            photonView.RPC("UpdateKeycardsDeliveredOthers", RpcTarget.Others);
        }

        keycardsDelivered++;

        if (keycardsDelivered == keycardRoles.Count)
        {
            foreach (DoorOpen door in doorsOut)
            {
                door.OpenClosedDoor();
            }
        }
    }

    [PunRPC]
    void UpdateKeycardsDeliveredOthers()
    {
        CheckPuzzleCompletion(false);
    }
}
