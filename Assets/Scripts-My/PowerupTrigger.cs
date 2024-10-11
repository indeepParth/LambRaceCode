using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PowerupTrigger : MonoBehaviour
{
    public PowerUpEnum PowerUp;
    public int spwanIndex = 0;
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
