using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Popup_ShowMessageOnly : UIPopUp
{
    public TextMeshProUGUI textMessage;

    void Awake()
    {
        base.OnEnable();
    }

    public void Init(string message)
    {
        textMessage.text = message;
    }

    public void Btn_ClosePopup()
    {
        HidePopUp(()=>
        {
            gameObject.SetActive(false);
        });
    }
}
