using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInfo : MonoBehaviour
{
    public int indexPref = 0;
    public Animator animator;
    public bool isPassanger = false;
    private int distToLook = 50;

    private void OnEnable()
    {
        if (!isPassanger)
        {
            int sp = Random.Range(1, 5);
            animator.SetFloat("Speed", sp);
        }
    }

    private void LateUpdate()
    {
        // if (!MyGameController.instance.isGameStart)
        //     return;
        if (MyGameController.instance.gameMode == GameMode.None)
            return;
            
        if (isPassanger && !MyGameController.instance.MyManager.pickupDropPassangerManager.lookAtCar)
            {
                return;
            }

        if(MyGameController.instance.MyManager == null || MyGameController.instance.MyManager.carLamb == null)
            return;

        float distance = Vector3.Distance(transform.position, MyGameController.instance.MyManager.carLamb.position);
        if(distance < distToLook)
        {
            Vector3 pos = MyGameController.instance.MyManager.carLamb.position;
            pos.y = 0;
            transform.LookAt(pos);
        }
    }

    // for passanger animation Event
    public void HandAnimationInCar(int h)
    {
        MyGameController.instance.MyManager.pickupDropPassangerManager.HandAnimationInCar(h == 0 ? true : false);
    }
    public void OpenCarDoor()
    {
        MyGameController.instance.MyManager.pickupDropPassangerManager.OpenCarDoor();
    }
    public void CloseCarDoor()
    {
        MyGameController.instance.MyManager.pickupDropPassangerManager.CloseCarDoor();
    }
    public void SetPassangerSittingHeight(float pos)
    {
        MyGameController.instance.MyManager.pickupDropPassangerManager.SetPassangerSittingHeight(pos);
    }
    public void EnableHeartParticleOfPassanger()
    {
        MyGameController.instance.MyManager.pickupDropPassangerManager.EnableHeartParticleOfPassanger();
    }
    //
}
