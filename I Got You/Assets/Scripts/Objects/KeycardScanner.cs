using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeycardScanner : InteractableObject
{
    private PuzzleEat puzzleEat;

    // Start is called before the first frame update
    protected override void AfterStart()
    {
        puzzleEat = GetComponentInParent<PuzzleEat>();
        interactText = " to scan Keycard";
    }

    protected override void PlayerTriggerEntered(PlayerStats playerStats)
    {
        base.PlayerTriggerEntered(playerStats);

        playerStats.OnInteract += CheckKeycardsInPossesion;
    }

    private void CheckKeycardsInPossesion(PlayerStats playerStats)
    {
        List<InteractableObject> cardsToRemove = new List<InteractableObject>();

        foreach (InteractableObject item in playerStats.InventoryOfInteractables)
        {
            if (item is KeycardObject)
            {
                puzzleEat.CheckPuzzleCompletion(true);
                cardsToRemove.Add(item);
            }
        }

        if (cardsToRemove.Count == 0)
        {
            playerUI.ShowNotification("No Keycards to scan!");
        }

        foreach (InteractableObject item in cardsToRemove)
        {
            playerStats.RemoveInteractable(item);
            Destroy(item);
        }
    }
}
