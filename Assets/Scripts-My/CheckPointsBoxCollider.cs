using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointsBoxCollider : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "car")
        {
            Debug.Log(this.gameObject.name + "Triggered");
            MyGameController.instance.MyManager.carLambController.OnCheckPointHit(this.gameObject.name);
        }
    }
}
