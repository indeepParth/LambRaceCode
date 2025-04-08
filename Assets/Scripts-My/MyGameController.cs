// This scripts should be used in ANY MAP/track ex. Miami, new york others
// use care fully with parameters
// do not use specific track/MAP parameters
using PG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using TurnTheGameOn.SimpleTrafficSystem;

public enum GameMode
{
    None,
    DateRush,
    GrandPrix,
    FreeRide
}

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

    private UILeaderboardPanel panel_Leaderboard;
    public UILeaderboardPanel Panel_Leaderboard
    {
        get
        {
            if (panel_Leaderboard == null)
            {
                panel_Leaderboard = transform.GetComponentInChildren<UILeaderboardPanel>(true);
            }
            return panel_Leaderboard;
        }
    }

    private Popup_ShowMessageOnly popup_ShowMessageOnly;
    public Popup_ShowMessageOnly Popup_ShowMessageOnly
    {
        get
        {
            if (popup_ShowMessageOnly == null)
            {
                popup_ShowMessageOnly = transform.GetComponentInChildren<Popup_ShowMessageOnly>(true);
            }
            return popup_ShowMessageOnly;
        }
    }

    private UILoginPanel uILoginPanel;
    public UILoginPanel UILoginPanel
    {
        get
        {
            if (uILoginPanel == null)
            {
                uILoginPanel = transform.GetComponentInChildren<UILoginPanel>(true);
            }
            return uILoginPanel;
        }
    }

    private Popup_GameInstructionAtEveryStart popup_GameInstructionAtEveryStart;
    public Popup_GameInstructionAtEveryStart Popup_GameInstructionAtEveryStart
    {
        get
        {
            if (popup_GameInstructionAtEveryStart == null)
            {
                popup_GameInstructionAtEveryStart = transform.GetComponentInChildren<Popup_GameInstructionAtEveryStart>(true);
            }
            return popup_GameInstructionAtEveryStart;
        }
    }





    public static MyGameController instance;
    public GameMode gameMode = GameMode.None;
    public int timer = 180;
    public int countDownTime = 120;
    public int increaseTimeReward = 60;
    float t = 0;
    public int counterUpTime = 0;

    public bool isGameOver;
    public bool isGameStart;
    public bool isGamePause;

    public TextMeshProUGUI textCountDown321;

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
    public void UpdateHeartPointOnPlayfab(int bestHeart)
    {
        PlayFabLogin.GameWonRewardEvent((respoce) =>
        {
            PlayFabLogin.SubmitBestHeartOnDropPessanger(bestHeart);
            UpdateHeartPoint(bestHeart);
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

    void Start()
    {
        ForceLogin();
    }

    public void ForceLogin()
    {
        if (!string.IsNullOrEmpty(PlayFabLogin.GetStoredWalletAddress()))
        {
            PlayFabLogin.LoginWithPlayfab();
        }
        else
        {
            UILoginPanel.gameObject.SetActive(true);
        }
    }

    public void ResetGame()
    {
        isGameOver = false;
        isGameStart = false;
        countDownTime = timer;
        counterUpTime = 0;
        UpdateHeartPoint(0);
    }

    public void RestartGame()
    {
        ResetGame();
        StartGame();
    }

    public void StartGame()
    {
        // CountDown321GoOnStart();
        Timer.Schedule(this, 1f, CountDown321GoOnStart);
        // isGameStart = true;
        // MySoundManager.RaceTrackSound(true);
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

        if (gameMode == GameMode.DateRush && isGameStart)
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
        else if (gameMode == GameMode.GrandPrix && isGameStart)
        {
            t = t + Time.deltaTime;
            if (t >= 1)
            {
                t = 0;
                counterUpTime = counterUpTime + 1;
                UIManager.UpdateCounterUpTimer(counterUpTime);
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

    public Ease easeType = Ease.Linear;
    public void CountDown321GoOnStart()
    {
        textCountDown321.text = "3";
        textCountDown321.gameObject.SetActive(true);
        MySoundManager.PlayBeep321();

        textCountDown321.transform.DOScale(Vector3.one * 1.5f, 0.5f).SetEase(easeType).OnComplete(() =>
        {
            textCountDown321.transform.DOScale(Vector3.one, 0.5f).SetEase(easeType).OnComplete(() =>
            {
                textCountDown321.text = "2";
                MySoundManager.PlayBeep321();
                textCountDown321.transform.DOScale(Vector3.one * 1.5f, 0.5f).SetEase(easeType).OnComplete(() =>
                {
                    textCountDown321.transform.DOScale(Vector3.one, 0.5f).SetEase(easeType).OnComplete(() =>
                    {
                        textCountDown321.text = "1";
                        MySoundManager.PlayBeep321();
                        textCountDown321.transform.DOScale(Vector3.one * 1.5f, 0.5f).SetEase(easeType).OnComplete(() =>
                        {
                            textCountDown321.transform.DOScale(Vector3.one, 0.5f).SetEase(easeType).OnComplete(() =>
                            {
                                textCountDown321.text = "GO";
                                MySoundManager.PlayBeep321();
                                textCountDown321.transform.DOScale(Vector3.one * 1.5f, 0.5f).SetEase(easeType).OnComplete(() =>
                                {
                                    textCountDown321.transform.DOScale(Vector3.one, 0.5f).SetEase(easeType).OnComplete(() =>
                                    {
                                        textCountDown321.gameObject.SetActive(false);
                                        // StartGame();
                                        MyManager.carLambController.UpdateBlockControl(false);
                                        isGameStart = true;
                                        MySoundManager.RaceTrackSound(true);
                                        AITrafficController.Instance.StartAITraffic();
                                    });
                                });
                            });
                        });
                    });
                });
            });
        });
    }

    public static string ShortenAddress(string address)
    {
        if (string.IsNullOrEmpty(address) || address.Length < 20)
            return address; // Return original if it's too short

        return $"{address.Substring(0, 8)}......{address.Substring(address.Length - 5)}";
    }
}

public static class CarParamsConstants
{
    //CarParams constants
    public const float MPHMult = 2.23693629f;
    public const float KPHMult = 5.3333f; //3.6f;
}
