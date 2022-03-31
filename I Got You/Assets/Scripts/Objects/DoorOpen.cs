using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    private PlayerManager playerManager;
    [SerializeField] private List<GameObject> playersInRange = new List<GameObject>();
    [SerializeField] private GameObject doorToClose;
    [SerializeField] private GameObject model;

    public delegate void OpenDoor();
    public OpenDoor OnOpenDoor;

    // Start is called before the first frame update
    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        doorToClose.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerCol"))
        {
            playersInRange.Add(other.gameObject);

            if (playerManager.Players.Count == playersInRange.Count)
            {
                doorToClose.SetActive(true);
                playersInRange.Clear();
                model.SetActive(false);
                OnOpenDoor?.Invoke();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerCol"))
        {
            playersInRange.Remove(other.gameObject);
        }
    }
}
