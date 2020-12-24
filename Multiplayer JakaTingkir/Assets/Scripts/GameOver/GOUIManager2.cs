using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GOUIManager2 : MonoBehaviour
{


    public void WinGame()
    {
        //win the game
        SceneManager.LoadScene(1);
    }

    public void Quitgame()
    {
        Application.Quit();
    }
}
