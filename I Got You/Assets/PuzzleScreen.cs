using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleScreen : MonoBehaviour
{
    [SerializeField]
    private int colourInt;
    [SerializeField]
    private GameObject screen;
    private Renderer matColour;
    private PuzzleManager manager;

    // Start is called before the first frame update
    void Start()
    {
        matColour = screen.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine("ShowSequence");
    }

    public IEnumerator ShowSequence()
    {
        for (int i = 0; i < manager.RandomInt.Count; i++)
        {
            colourInt = manager.RandomInt[i];
            switch (colourInt)
            {
                case 1:
                    matColour.material.color = Color.red;
                    break;

                case 2:
                    matColour.material.color = Color.green;
                    break;

                case 3:
                    matColour.material.color = Color.blue;
                    break;
                default:
                    break;
            }
            yield return new WaitForSeconds(2f); 
        }
        
    }
}
