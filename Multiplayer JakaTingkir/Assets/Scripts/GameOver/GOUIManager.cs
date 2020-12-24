using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GOUIManager : MonoBehaviour
{


    public void RestartGame()
    {
        //reset the game
        SceneManager.LoadScene(4);
    }

    public void Quitgame()
    {
        Application.Quit();
    }
}
