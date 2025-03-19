using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Popup_GameInstructionAtEveryStart : UIPopUp
{
    public GameObject textDateRush;
    public GameObject textGrandPrix;
    public GameObject textFreeMode;

    void Awake()
    {
        textDateRush.SetActive(false);
        textGrandPrix.SetActive(false);
        textFreeMode.SetActive(false);
        Init();
        base.OnEnable();
    }

    public void Init()
    {
        switch (MyGameController.instance.gameMode)
        {
            case GameMode.DateRush:
                textDateRush.SetActive(true);
                break;
            case GameMode.GrandPrix:
                textGrandPrix.SetActive(true);
                break;
            case GameMode.FreeRide:
                textFreeMode.SetActive(true);
                break;
        }
    }

    public void Btn_ClosePopup()
    {
        HidePopUp(()=>
        {
            MyGameController.instance.StartGame();
            gameObject.SetActive(false);
        });
    }
}
