using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCameraFollow : MonoBehaviour
{
    public Transform target; // The car to follow
    public Vector3 offset; // Offset from the car
    public float followSpeed = 10f; // Speed at which the camera follows the car
    public float rotateSpeed = 5f; // Speed at which the camera rotates to match the car's rotation
    private bool isFocusOnCar = false;
    private Vector3 lastCamPosOnCar = Vector3.zero;
    public Vector3 offsetPassanger;

    public Camera myCamera;

    private void Start()
    {
        lastCamPosOnCar = transform.position;
        OnCameraFocusChange(true);
    }

    void LateUpdate()
    {
        if (isFocusOnCar)
        {
            Vector3 playerForward = (MyGameController.instance.MyManager.carLambRigidbody.velocity + target.transform.forward).normalized;
            transform.position = Vector3.Lerp(transform.position,
                target.position + target.transform.TransformVector(offset)
                + playerForward * (-rotateSpeed),
                followSpeed * Time.deltaTime);

            Debug.Log("playerForward" + playerForward);
            // if (playerForward.z > 0 && !MyGameController.instance.MyManager.carLambController.isReverse)
            // { MyGameController.instance.MyManager.carLambController.OnReverseUpdateCarChildPosition(false); }
            // else if (playerForward.z < 0 && MyGameController.instance.MyManager.carLambController.isReverse)
            // { MyGameController.instance.MyManager.carLambController.OnReverseUpdateCarChildPosition(true); }

            lastCamPosOnCar = transform.position;
        }
        else
        {
            Vector3 playerForward = target.transform.forward.normalized;
            transform.position = Vector3.Lerp(transform.position,
                target.position + target.transform.TransformVector(offsetPassanger)
                + playerForward * (-rotateSpeed),
                followSpeed * Time.deltaTime);
        }

        transform.LookAt(target);
    }

    public void OnCameraFocusChange(bool _isFocusOnCar)
    {        
        if(!_isFocusOnCar && MyGameController.instance.MyManager.pickupDropPassangerManager.passanger != null)
        {
            isFocusOnCar = false;
            target = MyGameController.instance.MyManager.pickupDropPassangerManager.passanger.transform;
            //Vector3 _pos = MyGameController.instance.MyManager.carLamb.position;
            //_pos.y = transform.position.y + 3;
            //transform.DOMove(_pos, 0.5f);
            myCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("carsAI"));            
        }
        else
        {
            transform.DOMove(lastCamPosOnCar, 0);
            isFocusOnCar = true;
            target = MyGameController.instance.MyManager.carLamb;
            myCamera.cullingMask |= 1 << LayerMask.NameToLayer("carsAI");
        }
    }
}
