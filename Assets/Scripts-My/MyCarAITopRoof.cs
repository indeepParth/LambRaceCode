using System.Collections;
using System.Collections.Generic;
using TurnTheGameOn.SimpleTrafficSystem;
using UnityEngine;

public class MyCarAITopRoof : MonoBehaviour
{
    public AITrafficCar aITrafficCar;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("car"))
        {
            aITrafficCar.HideAfterAccident();
        }
    }
}
