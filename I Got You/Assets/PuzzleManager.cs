using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> screens;
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
    public int Score { get { return score; } }
    // Start is called before the first frame update
    void Start()
    {
        foreach (int randomInt in randomInt)
        {
            Debug.Log(randomInt.ToString());
        }
    }
    public void ScreenSequence()
    {
        randomInt.Clear();

        while (randomInt.Count < maxSequence)
        {
            randomInt.Add(Random.Range(1, 4));
        }
        StartCoroutine(ShowSequence());
    }

    IEnumerator ShowSequence()
    {
        playerInput.Clear();
        int screenIndex = 0;

        for (int i = 0; i < randomInt.Count; i++)
        {
         //   screens[0].ColourInt = randomInt[i];

            if (randomInt[i] == screens[0].GetComponent<PuzzleScreen>().ColourInt)
            {
                screenIndex = 0;
                screens[0].GetComponent<PuzzleScreen>().MatColour.material.color = Color.red;
            }

            if (randomInt[i] == screens[1].GetComponent<PuzzleScreen>().ColourInt)
            {
                screenIndex = 1;
                screens[1].GetComponent<PuzzleScreen>().MatColour.material.color = Color.green;
            }

            if (randomInt[i] == screens[2].GetComponent<PuzzleScreen>().ColourInt)
            {
                screenIndex = 2;
                screens[2].GetComponent<PuzzleScreen>().MatColour.material.color = Color.blue;
            }
            /*   switch (screens[0].ColourInt)
               {
                   case 1:
                       screens[0].MatColour.material.color = Color.red;
                       break;

                   case 2:
                       screens[1].MatColour.material.color = Color.green;
                       break;

                   case 3:
                       screens[2].MatColour.material.color = Color.blue;
                       break;
                   default:
                       break;
               } */
            yield return new WaitForSeconds(2f);
            screens[screenIndex].GetComponent<PuzzleScreen>().MatColour.material.color = Color.black;
            yield return new WaitForSeconds(2f);
        }
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
            score = 0;
            randomInt.Clear();
            StopCoroutine(ShowSequence());
            return;
        }

        if (score >= randomInt.Count)
        {
            openDoor = true;
        }
    }

}
