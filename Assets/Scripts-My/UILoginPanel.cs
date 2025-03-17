using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class UILoginPanel : MonoBehaviour
{
    public TMP_InputField textPlayerName;
    public TMP_InputField textWalletAddress;

    public GameObject obj_invalidName;
    public GameObject obj_invalidWallet;

    private bool nameOK, walletOK;

    void Awake()
    {
        obj_invalidName.SetActive(false);
        obj_invalidWallet.SetActive(false);
        textPlayerName.text = "";
        textWalletAddress.text = "";
    }

    public void Btn_Login()
    {
        CheckPlayerName();
        CheckWalletAddress();
        if (nameOK && walletOK)
        {
            MyGameController.instance.PlayFabLogin.LoginWithPlayfab();
            // Debug.LogWarning("LoginWithPlayfab() is not implemented yet");
        }
    }

    private void CheckPlayerName()
    {
        nameOK = false;
        string name = textPlayerName.text;
        if (!string.IsNullOrEmpty(name) && name.Length > 0)
        {
            PlayerPrefs.SetString(Utility.KEY_PLAYERNAME, name);
            nameOK = true;
        }
        else
        {
            Debug.LogWarning("Invalid player name");
            obj_invalidName.SetActive(true);
        }
    }

    private void CheckWalletAddress()
    {
        walletOK = false;
        string address = textWalletAddress.text;

        // Check if address is not null, starts with "0x" and has exactly 42 characters
        if (string.IsNullOrEmpty(address) || !address.StartsWith("0x") || address.Length != 42)
        {
            walletOK = false;
        }
        else
        {
            string hexPattern = @"^0x[a-fA-F0-9]{40}$";
            walletOK = Regex.IsMatch(address, hexPattern);
        }

        if (walletOK)
        {
            PlayerPrefs.SetString(Utility.KEY_WALLET_ID, address);
        }
        else
        {
            Debug.LogWarning("Invalid wallet address");
            obj_invalidWallet.SetActive(true);
        }
    }

    public void OnEditTextName()
    {
        obj_invalidName.SetActive(false);
    }

    public void OnEditTextWallet()
    {
        obj_invalidWallet.SetActive(false);
    }

    public void Btn_HelpOnMetamaskAddress()
    {
        MyGameController.instance.Popup_ShowMessageOnly.Init("Open your metamask,\ncopy your wallet address,\npaste it here.");
        MyGameController.instance.Popup_ShowMessageOnly.gameObject.SetActive(true);
    }
}
