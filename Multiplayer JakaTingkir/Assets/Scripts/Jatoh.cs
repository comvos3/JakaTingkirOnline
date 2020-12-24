using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Jatoh : MonoBehaviour
{
    public Transform Destination;

    void OnTriggerEnter2D(Collider2D other)
    {
        
        other.transform.position = Destination.position;
    }
}
