using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PindahTempat : MonoBehaviour
{

    public void OnPlayerEntered(Player player)
    {
        if (player != null)
        {
            SceneManager.LoadScene(5);
        }
    }
}
