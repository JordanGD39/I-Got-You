using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleScreen : MonoBehaviour
{
    [SerializeField]
    private int colourInt;
    public int ColourInt { get { return colourInt; } }
    [SerializeField]
    private GameObject screen;
    private MeshRenderer matColour;
    public MeshRenderer MatColour { get { return matColour; } }
    private PuzzleManager manager;

    // Start is called before the first frame update
    void Start()
    {
        matColour = screen.GetComponent<MeshRenderer>();
    }

  /*  public IEnumerator ShowSequence()
    {
        for (int i = 0; i < manager.RandomInt.Count; i++)
        {
            colourInt = manager.RandomInt[i];
            
            }
            yield return new WaitForSeconds(2f); 
        }
    } */
}
