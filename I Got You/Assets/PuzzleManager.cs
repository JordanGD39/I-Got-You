using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> screens;
    [SerializeField]
    private List<int> randomInt;
    private int sequenceLength;
    [SerializeField]
    private int maxSequence;
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
        while (randomInt.Count < maxSequence)
        {
            randomInt.Add(Random.Range(1, 4));
        }
        StartCoroutine(ShowSequence());
    }

    IEnumerator ShowSequence()
    {
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
}
