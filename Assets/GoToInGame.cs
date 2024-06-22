using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToInGame : MonoBehaviour
{
    public void Execute()
    {
        SceneManager.LoadScene("InGame");
    }
}
