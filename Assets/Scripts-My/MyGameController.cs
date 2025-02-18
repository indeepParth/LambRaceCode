// This scripts should be used in ANY MAP/track ex. Miami, new york others
// use care fully with parameters
// do not use specific track/MAP parameters
using PG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MyGameController : MonoBehaviour
{
    private MyManager myManager;
    public MyManager MyManager
    {
        get
        {
            if (myManager == null)
            {
                myManager = FindFirstObjectByType<MyManager>();
            }
            return myManager;
        }
    }

    private MyCameraFollow myCameraFollow;
    public MyCameraFollow MyCameraFollow
    {
        get
        {
            if (myCameraFollow == null)
            {
                myCameraFollow = FindFirstObjectByType<MyCameraFollow>();
            }
            return myCameraFollow;
        }
    }

    private UIManager uIManager;
    public UIManager UIManager
    {
        get
        {
            if (uIManager == null)
            {
                uIManager = transform.GetComponentInChildren<UIManager>(true);
            }
            return uIManager;
        }
    }

    private MySoundManager mySoundManager;
    public MySoundManager MySoundManager
    {
        get
        {
            if (mySoundManager == null)
            {
                mySoundManager = transform.GetComponentInChildren<MySoundManager>(true);
            }
            return mySoundManager;
        }
    }

    private MyCarStateUI myCarStateUI;
    public MyCarStateUI MyCarStateUI
    {
        get
        {
            if (myCarStateUI == null)
            {
                myCarStateUI = transform.GetComponentInChildren<MyCarStateUI>(true);
            }
            return myCarStateUI;
        }
    }

    private PlayFabLogin playFabLogin;
    public PlayFabLogin PlayFabLogin
    {
        get
        {
            if (playFabLogin == null)
            {
                playFabLogin = transform.GetComponentInChildren<PlayFabLogin>(true);
            }
            return playFabLogin;
        }
    }

    private Panel_Leaderboard panel_Leaderboard;
    public Panel_Leaderboard Panel_Leaderboard
    {
        get
        {
            if (panel_Leaderboard == null)
            {
                panel_Leaderboard = transform.GetComponentInChildren<Panel_Leaderboard>(true);
            }
            return panel_Leaderboard;
        }
    }





    public static MyGameController instance;
    public bool freeMode = false;
    public int timer = 180;
    public int countDownTime = 120;
    public int increaseTimeReward = 60;
    float t = 0;

    public bool isGameOver;
    public bool isGameStart;
    public bool isGamePause;

    public int rewardHeartPoint = 1;
    public int HeartPoint
    {
        get
        {
            return PlayerPrefs.GetInt("heartPoint", 0);
        }
        set
        {
            PlayerPrefs.SetInt("heartPoint", value);
        }
    }
    public void UpdateHeartPointOnPlayfab()
    {
        PlayFabLogin.GameWonRewardEvent((respoce) =>
        {
            UpdateHeartPoint(respoce);
        });        
    }
    public void UpdateHeartPoint(int respoce)
    {
        HeartPoint = respoce;
        UIManager.UpdateHeartPointText(HeartPoint);
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Application.targetFrameRate = 60;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetGame()
    {
        isGameOver = false;
        isGameStart = false;
        countDownTime = timer;
    }

    public void RestartGame()
    {
        ResetGame();
        StartGame();
    }

    public void StartGame()
    {
        isGameStart = true;
        MySoundManager.RaceTrackSound(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }

        if (isGameOver || isGamePause)
            return;

        if (!freeMode && isGameStart)
        {
            t = t + Time.deltaTime;
            if (t >= 1)
            {
                t = 0;
                countDownTime = countDownTime - 1;
                UIManager.UpdateCountDownTimer(countDownTime);
                if (countDownTime <= 0)
                {
                    isGameOver = true;
                    MySoundManager.RaceTrackSound(false);
                    MyManager.carLambController.UpdateBlockControl(true);
                    UIManager.ShowGameOver();
                }
            }
        }
    }

    public void IncreaseTimerAsReward(int timeBonus)
    {
        countDownTime = countDownTime + timeBonus;
        UIManager.timeBonusTextAnimAdd.text = $"+{timeBonus}s";
        HideTimeBonusTextAnimAdd();
    }
    private void HideTimeBonusTextAnimAdd()
    {
        RectTransform _rectTransform = UIManager.timeBonusTextAnimAdd.GetComponent<RectTransform>();        
        _rectTransform.DOAnchorPosY(-40, 0).OnComplete(() =>
        {
            UIManager.timeBonusTextAnimAdd.DOFade(1, 0);
            UIManager.timeBonusTextAnimAdd.gameObject.SetActive(true);
            _rectTransform.DOAnchorPosY(0, 0.5f).SetDelay(1).SetEase(Ease.Linear).OnComplete(() =>
            {
                UIManager.timeBonusTextAnimAdd.gameObject.SetActive(false);
            });
            UIManager.timeBonusTextAnimAdd.DOFade(0, 0.4f).SetDelay(1);
        });
    }
}

public static class CarParamsConstants
{
    //CarParams constants
    public const float MPHMult = 2.23693629f;
    public const float KPHMult = 5.3333f; //3.6f;
}
