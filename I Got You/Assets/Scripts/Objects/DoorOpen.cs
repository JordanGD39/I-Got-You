using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    private PlayerManager playerManager;
    [SerializeField] private List<GameObject> playersInRange = new List<GameObject>();
    [SerializeField] private GameObject doorToClose;
    [SerializeField] private GameObject model;
    [SerializeField] private bool opened = true;
    [SerializeField] private Animator openingDoorAnim;
    [SerializeField] private Animator closingDoorAnim;

    public delegate void OpenDoor();
    public OpenDoor OnOpenDoor;

    // Start is called before the first frame update
    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        doorToClose.SetActive(false);
        opened = true;
        playersInRange.Clear();
        Invoke(nameof(OpenResetDelay), 0.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (opened)
        {
            return;
        }

        if (other.CompareTag("PlayerCol"))
        {
            if (playerManager.StatsOfAllPlayers[other].IsDead)
            {
                return;
            }

            playersInRange.Add(other.gameObject);

            if (playersInRange.Count >= playerManager.Players.Count)
            {
                Debug.Log(playersInRange.Count);
                opened = true;
                doorToClose.SetActive(true);
                openingDoorAnim.ResetTrigger("Open");
                openingDoorAnim.SetTrigger("Open");
                playersInRange.Clear();
                closingDoorAnim.ResetTrigger("Close");
                closingDoorAnim.SetTrigger("Close");
                OnOpenDoor?.Invoke();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (opened)
        {
            return;
        }

        if (other.CompareTag("PlayerCol") && playersInRange.Contains(other.gameObject))
        {
            playersInRange.Remove(other.gameObject);
        }
    }

    public void CloseOpeningDoor()
    {
        model.SetActive(false);
        model.SetActive(true);
    }

    public void ResetDoor()
    {
        doorToClose.SetActive(false);
        model.SetActive(false);
        model.SetActive(true);

        Invoke(nameof(OpenResetDelay), 0.5f);
    }

    private void OpenResetDelay()
    {
        opened = false;
    }
}
