using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField]
    private List<PuzzleScreen> screens;
    [SerializeField]
    private List<int> randomInt;
    public List<int> RandomInt { get; set; }
    private int sequenceLength;
    [SerializeField]
    private int maxSequence;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
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
            randomInt.Add(Random.Range(0, 4));
        }

    }
}
