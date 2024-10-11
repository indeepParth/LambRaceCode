using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxPickupDropTrigger : MonoBehaviour
{
    private bool isTriggerOn = true;

    private void OnEnable()
    {
        isTriggerOn = true;
    }
    private void OnDisable()
    {
        isTriggerOn = true;
    }

    float t = 0;
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("car") && isTriggerOn)
        {
            if (MyGameController.instance.MyManager.carLambController.currentSpeed < 15)
            {
                MyGameController.instance.MyManager.carLambController.StopCarInPickupDropArea();
                t = t + Time.deltaTime;
                if (t > 1)
                {
                    t = 0;
                    isTriggerOn = false;
                    //MyGameController.instance.MyManager.carLambRigidbody.drag = 20;
                    MyGameController.instance.MyManager.carLambController.UpdateBlockControl(true);
                    //Timer.Schedule(this, 1, MyGameController.instance.MyManager.pickupDropPassangerManager.OnCarTriggerPickupDropLocation);
                    MyGameController.instance.MyManager.pickupDropPassangerManager.OnCarTriggerPickupDropLocationDelay();
                }
            }
            else
            {
                t = 0;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("car"))
        {
            isTriggerOn = true;
            MyGameController.instance.MyManager.carLambRigidbody.drag = 0.02f;
        }
    }
}
