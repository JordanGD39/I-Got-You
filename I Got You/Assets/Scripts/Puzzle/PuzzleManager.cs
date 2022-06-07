using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PuzzleManager : MonoBehaviourPun
{
    [SerializeField]
    private List<GameObject> screens;
    [SerializeField] private GameObject monitor;
    [SerializeField]
    private List<int> playerInput;
    public List<int> PlayerInput { get { return playerInput; } }
    [SerializeField]
    private List<int> randomInt;
    public List<int> RandomInt { get { return randomInt; } }
    private int sequenceLength;
    [SerializeField]
    private int maxSequence;
    private int score;
    [SerializeField]
    private bool openDoor = false;
    public bool OpenDoor { get { return openDoor; } }
    [SerializeField] private RoomManager roomManager;
    public int Score { get { return score; } }
    public bool ShownPuzzle { get; set; } = false;
    [SerializeField] private Texture checkTexture;
    [SerializeField] private Texture crossTexture;
    [SerializeField] private Texture showingTexture;

    private PlayerManager playerManager;

    // Start is called before the first frame update
    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();

        foreach (int randomInt in randomInt)
        {
            Debug.Log(randomInt.ToString());
        }
    }

    public void RemoveScreensWhenEnteredRoom()
    {
        RemoveScreensBasedOnPlayerCount(true);
    }

    private void RemoveScreensBasedOnPlayerCount(bool roomEntered)
    {
        if (openDoor)
        {
            return;
        }

        int screensToRemove = 4 - (roomEntered ? playerManager.PlayersInGame.Count : playerManager.Players.Count);
        screensToRemove = Mathf.Clamp(screensToRemove, 0, 2);

        switch (screensToRemove)
        {
            case 1:
                if (roomEntered)
                {
                    screens[2].SetActive(false);
                }
                
                screens[2] = null;
                break;
            case 2:
                if (roomEntered)
                {
                    screens[1].SetActive(false);
                    screens[2].SetActive(false);
                }

                screens[1] = null;
                screens[2] = null;
                break;
            case 3:
                if (roomEntered)
                {
                    screens[0].SetActive(false);
                    screens[1].SetActive(false);
                    screens[2].SetActive(false);
                }

                screens[0] = monitor;
                screens[1] = null;
                screens[2] = null;
                break;
        }
    }

    public void ScreenSequence(bool localPlayer)
    {
        RemoveScreensBasedOnPlayerCount(false);
        MeshRenderer meshRenderer = monitor.GetComponent<MeshRenderer>();
        meshRenderer.material.color = Color.white;
        meshRenderer.material.mainTexture = showingTexture;
        ShownPuzzle = false;

        randomInt.Clear();    

        while (randomInt.Count < maxSequence)
        {
            randomInt.Add(Random.Range(1, 4));
        }

        if (localPlayer && PhotonNetwork.IsConnected)
        {
            photonView.RPC("StartScreenSequenceOthers", RpcTarget.Others, randomInt.ToArray());
        }

        StartCoroutine(ShowSequence());
    }

    [PunRPC]
    void StartScreenSequenceOthers(int[] ints)
    {
        randomInt = new List<int>(ints);

        StartCoroutine(ShowSequence());
    }

    IEnumerator ShowSequence()
    {
        playerInput.Clear();
        int screenIndex = 0;

        for (int i = 0; i < randomInt.Count; i++)
        {
         //   screens[0].ColourInt = randomInt[i];

            if (randomInt[i] == 1)
            {
                screenIndex = 0;

                if (screens[0].GetComponent<PuzzleScreen>() != null)
                {
                    screens[0].GetComponent<PuzzleScreen>().MatColour.material.color = Color.red;
                }
                else
                {
                    screens[0].GetComponent<MeshRenderer>().material.color = Color.red;
                }
            }
            else if (randomInt[i] == 2)
            {
                screenIndex = 1;

                if (screens[1] == null)
                {
                    screenIndex = 0;
                }

                if (screens[screenIndex].GetComponent<PuzzleScreen>() != null)
                {
                    screens[screenIndex].GetComponent<PuzzleScreen>().MatColour.material.color = Color.green;
                }
                else
                {
                    screens[screenIndex].GetComponent<MeshRenderer>().material.color = Color.green;
                }
            }
            else if (randomInt[i] == 3)
            {
                screenIndex = 2;

                if (screens[2] == null)
                {
                    screenIndex = 1;

                    if (screens[1] == null)
                    {
                        screenIndex = 0;
                    }
                }

                if (screens[screenIndex].GetComponent<PuzzleScreen>() != null)
                {
                    screens[screenIndex].GetComponent<PuzzleScreen>().MatColour.material.color = Color.blue;
                }
                else
                {
                    screens[screenIndex].GetComponent<MeshRenderer>().material.color = Color.blue;
                }
            }

            yield return new WaitForSeconds(0.5f);
            if (screens[screenIndex].GetComponent<PuzzleScreen>() != null)
            {
                screens[screenIndex].GetComponent<PuzzleScreen>().MatColour.material.color = Color.black;
            }
            else
            {
                screens[screenIndex].GetComponent<MeshRenderer>().material.color = Color.black;
            }
            yield return new WaitForSeconds(0.5f);
        }

        ShownPuzzle = true;
    }

    public void CheckCorrectStep()
    {
        if (playerInput.Count > randomInt.Count)
        {
            return;
        }

        if (playerInput[playerInput.Count - 1] == randomInt[playerInput.Count - 1])
        {
            score++;
        }
        else
        {
            MeshRenderer meshRenderer = monitor.GetComponent<MeshRenderer>();
            meshRenderer.material.color = Color.white;
            meshRenderer.material.mainTexture = crossTexture;
            ShownPuzzle = false;
            score = 0;
            randomInt.Clear();
            StopCoroutine(ShowSequence());
            return;
        }

        if (score >= randomInt.Count)
        {
            Invoke(nameof(ShowCheck), 1);
            openDoor = true;
            roomManager.OpenAllDoors();
        }
    }

    private void ShowCheck()
    {
        MeshRenderer meshRenderer = monitor.GetComponent<MeshRenderer>();
        meshRenderer.material.color = Color.white;
        meshRenderer.material.mainTexture = checkTexture;
    }
}
