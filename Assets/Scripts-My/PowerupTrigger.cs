using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;



public class PowerupTrigger : MonoBehaviour
{
    public PowerUpEnum PowerUp;
    public int spwanIndex = 0;

    void Start()
    {
        if (PowerUp == PowerUpEnum.TimeBonus)
        {
            transform.DOLocalRotate(new Vector3(0, 180, 0), 1).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("car"))
        {
            if(PowerUp == PowerUpEnum.Booster)
            {
                MyGameController.instance.MyManager.carLambController.powerUps.ResetBoosterOnTrigger();
            }
            else if(PowerUp == PowerUpEnum.TimeBonus)
            {
                gameObject.SetActive(false);
                MyGameController.instance.MyManager.carLambController.powerUps.TimeBonusOnTrigger(spwanIndex);
            }
        }
    }
}
