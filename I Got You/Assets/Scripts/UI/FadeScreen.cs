using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeScreen : MonoBehaviour
{
    [SerializeField] private Animator fadeAnim;
    [SerializeField] private GameObject fadeInScreen;

    // Start is called before the first frame update
    void Start()
    {
        fadeInScreen.SetActive(false);

        DungeonGenerator dungeonGenerator = FindObjectOfType<DungeonGenerator>();

        if (dungeonGenerator.StartingRoom == null)
        {
            dungeonGenerator.OnGenerationDone += TriggerFadeOut;
        }
        else
        {
            TriggerFadeOut();
        }
    }

    private void TriggerFadeOut()
    {
        fadeAnim.SetTrigger("FadeOut");
    }

    public void TriggerFadeIn()
    {
        fadeInScreen.SetActive(true);
    }
}
