using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenUI : MonoBehaviour
{
    // Start is called before the first frame update
    public void GoToNextScene()
    {
        SceneManager.LoadScene("Connecting");
    }
}
