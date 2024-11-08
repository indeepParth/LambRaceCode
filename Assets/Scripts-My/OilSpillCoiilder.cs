using System.Collections;
using System.Collections.Generic;
using PG;
using UnityEngine;

public class OilSpillCoiilder : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("car"))
        {
            MyGameController.instance.MyManager.carLambController.enableOilSpillEffect = true;           
        }
    }
}
