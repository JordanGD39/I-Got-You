using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SupportBurstHeal : MonoBehaviourPun
{
    private int soupToRemove = 40;
    [SerializeField] private float charge = 0;
    public float Charge { get { return charge; } }
    public GameObject BurstArea { get; set; } = null;
    private HealSphere healSphere;

    private PlayerUI playerUI;
    private PlayerHealing playerHealing;

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine)
        {
            return;
        }

        GetComponent<PlayerStats>().SupportBurstHealScript = this;
        playerUI = FindObjectOfType<PlayerUI>();
        playerHealing = GetComponent<PlayerHealing>();

        if (PlayersStatsHolder.instance.PlayerStatsSaved.Length > 0 && PlayersStatsHolder.instance.PlayerStatsSaved[!PhotonNetwork.IsConnected ? 0 : photonView.OwnerActorNr - 1] != null)
        {
            charge = PlayersStatsHolder.instance.PlayerStatsSaved[!PhotonNetwork.IsConnected ? 0 : photonView.OwnerActorNr - 1].abilityCharge;
        }

        playerUI.UpdateAbility(2, charge, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        CheckAbilityToggle();
    }

    private void CheckAbilityToggle()
    {
        if (Input.GetButtonDown("Ability") && charge >= 1)
        {
            if (playerHealing.HealingItems == 0)
            {
                playerUI.ShowNotification("Need chicken soup to activate Support ability!");
                Invoke(nameof(HideNotification), 1);
                return;
            }

            if (healSphere == null)
            {
                healSphere = BurstArea.GetComponent<HealSphere>();
            }

            playerUI.UpdateAbility(2, 0, 1);
            playerHealing.RemoveSoup(soupToRemove);
            charge = 0;
            healSphere.OnFoundAllInRange = HealAllInRange;
            BurstArea.SetActive(true);
        }
    }

    public void AddCharge(float add)
    {
        charge += add;
        playerUI.UpdateAbility(2, charge, 1);
    }

    private void HideNotification()
    {
        playerUI.HideInteractPanel();
    }

    private void HealAllInRange(List<PlayerStats> inRange)
    {
        foreach (PlayerStats player in inRange)
        {
            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC("HealPlayerWithID", RpcTarget.Others, player.photonView.ViewID);
            }

            player.GetComponent<PlayerHealing>().PlayDrinkEffect();
        }
    }

    [PunRPC]
    void HealPlayerWithID(int viewId)
    {
        GameObject player = PhotonNetwork.GetPhotonView(viewId).gameObject;
        player.GetComponent<PlayerStats>().Heal(soupToRemove);
        player.GetComponent<PlayerHealing>().PlayDrinkEffect();
    }
}
