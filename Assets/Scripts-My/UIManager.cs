
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject homePanel;
    public GameObject gameDateRushPanel;
    public GameObject gameGrandPrixPanel;
    public GameObject gameFreeRidePanel;
    public GameObject gameOverPanel;

    [Space(2)]
    [Header("Date Rush")]
    public TextMeshProUGUI countDownText;
    public TextMeshProUGUI timeBonusTextAnimAdd;

    [Space(2)]
    [Header("Grand Prix")]
    public TextMeshProUGUI counterUPText;

    [Space(2)]
    [Header("Other")]
    public RectTransform img_BoostReaction;
    private bool boostReactionAnimPlaying = false;
    public RectTransform img_BoostAvailable;
    private bool boostAvailable;
    public TextMeshProUGUI heartPointText;

    private void Start()
    {
        MyGameController.instance.ResetGame();
        homePanel.SetActive(true);
        gameFreeRidePanel.SetActive(false);
        gameDateRushPanel.SetActive(false);
        gameGrandPrixPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        IsBoostAvailable(false);
    }

    public void SceneLoader(int index, UIScreenBlend.UISVoid callback = null)
    {
        UIScreenBlend.Instance.DarkIn(() =>
        {
            SceneManager.LoadSceneAsync(index).completed += (obj) =>
            {
                MyGameController.instance.ResetGame();
                UIScreenBlend.Instance.DarkOut(() =>
                {
                    MyGameController.instance.MyCarStateUI.gameObject.SetActive(index == 0 ? false : true);
                });
                if (callback != null)
                {
                    callback();
                }
            };
        });
    }

    //  Home UI
    public void BTN_FreeRideMode()
    {
        // MyGameController.instance.freeMode = true;
        MyGameController.instance.gameMode = GameMode.FreeRide;
        MyGameController.instance.MySoundManager.BackgroundOnHomeSound(false);
        SceneLoader(1, () =>
        {
            MyGameController.instance.StartGame();
            gameFreeRidePanel.SetActive(true);
            gameDateRushPanel.SetActive(false);
            gameGrandPrixPanel.SetActive(false);
            homePanel.SetActive(false);
            IsBoostAvailable(false);
            MyGameController.instance.MyManager.grandPrixCheckPoints.SetActive(false);
        });
    }

    public void BTN_GrandPrixMode()
    {
        // MyGameController.instance.freeMode = true;
        MyGameController.instance.gameMode = GameMode.GrandPrix;
        MyGameController.instance.MySoundManager.BackgroundOnHomeSound(false);
        SceneLoader(1, () =>
        {
            MyGameController.instance.StartGame();
            gameFreeRidePanel.SetActive(false);
            gameDateRushPanel.SetActive(false);
            gameGrandPrixPanel.SetActive(true);
            homePanel.SetActive(false);
            IsBoostAvailable(false);
            MyGameController.instance.MyManager.textStartFinish.text = "START";
            MyGameController.instance.MyManager.textStartFinish.gameObject.SetActive(true);
            MyGameController.instance.MyManager.grandPrixCheckPoints.SetActive(true);
        });
    }

    public void BTN_MiamiCrazyTextMode()
    {
        // MyGameController.instance.freeMode = false;
        MyGameController.instance.gameMode = GameMode.DateRush;
        MyGameController.instance.MySoundManager.BackgroundOnHomeSound(false);
        SceneLoader(1, () =>
        {
            MyGameController.instance.StartGame();
            gameDateRushPanel.SetActive(true);
            gameFreeRidePanel.SetActive(false);
            gameGrandPrixPanel.SetActive(false);
            homePanel.SetActive(false);
            IsBoostAvailable(false);
            MyGameController.instance.MyManager.grandPrixCheckPoints.SetActive(false);
        });
    }

    public void Btn_HomeOnGameOver()
    {
        MyGameController.instance.gameMode = GameMode.None;
        MyGameController.instance.MySoundManager.RaceTrackSound(false);
        MyGameController.instance.MyManager.boxCharacterTrigger.CharacterDisableOnSceneHome();
        SceneLoader(0, () =>
        {
            gameOverPanel.SetActive(false);
            gameDateRushPanel.SetActive(false);
            gameFreeRidePanel.SetActive(false);
            gameGrandPrixPanel.SetActive(false);
            homePanel.SetActive(true);
            IsBoostAvailable(false);
            MyGameController.instance.MySoundManager.BackgroundOnHomeSound(true);
        });
    }
    //

    // IN Game UI
    public void UpdateCountDownTimer(int time) // date rush mode
    {
        countDownText.text = time.ToString() + "s";
    }

    public void UpdateCounterUpTimer(int time) // grand prix mode
    {
        counterUPText.text = time.ToString() + "s";
    }

    // Over UI
    public void Btn_RestartOnGameOver()
    {
        SceneLoader(SceneManager.GetActiveScene().buildIndex, () =>
        {
            MyGameController.instance.StartGame();
            gameOverPanel.SetActive(false);
            if (MyGameController.instance.gameMode == GameMode.FreeRide)
            {
                gameFreeRidePanel.SetActive(true); gameDateRushPanel.SetActive(false);
            }
            else
            {
                gameFreeRidePanel.SetActive(false); gameDateRushPanel.SetActive(true);
            }
        });
    }

    public void ShowGameOver()
    {
        gameDateRushPanel.SetActive(false);
        gameOverPanel.SetActive(true);
        MyGameController.instance.MyManager.pickupDropPassangerManager.PickupDropPoint.SetActive(false);
    }

    public void BoostNitroReaction()
    {
        if (boostReactionAnimPlaying)
            return;

        IsBoostAvailable(false);
        boostReactionAnimPlaying = true;
        img_BoostReaction.DOAnchorPosX(150, 0).OnComplete(() =>
        {
            img_BoostReaction.gameObject.SetActive(true);
            img_BoostReaction.DOAnchorPosX(-10, 0.5f).SetEase(Ease.InOutExpo).OnComplete(() =>
            {
                MyGameController.instance.MySoundManager.PlayWohooo();
                img_BoostReaction.DOAnchorPosX(150, 0.5f).SetDelay(1.5f).SetEase(Ease.InOutExpo).OnComplete(() =>
                {
                    img_BoostReaction.gameObject.SetActive(false);
                    boostReactionAnimPlaying = false;
                });
            });
        });

    }

    public void IsBoostAvailable(bool _boostAvailable)
    {
        if (_boostAvailable && !boostAvailable)
        {
            boostAvailable = true;
            img_BoostAvailable.DOAnchorPosX(-10, 0.5f).SetEase(Ease.InOutExpo);
            MyGameController.instance.MyCarStateUI.ResetBoostWhenAvailable();
        }
        else if (!_boostAvailable)
        {
            boostAvailable = false;
            img_BoostAvailable.DOAnchorPosX(150, 0.5f).SetEase(Ease.InOutExpo);
        }
    }

    public void UpdateHeartPointText(int point)
    {
        heartPointText.text = point.ToString();
    }

    public void Btn_ShowLeaderboard()
    {
        MyGameController.instance.Panel_Leaderboard.gameObject.SetActive(true);
        MyGameController.instance.Panel_Leaderboard.Btn_leaderboard(0);
    }

    public void Btn_LogOut()
    {
        PlayerPrefs.DeleteAll();
        SceneLoader(0, () =>
        {
            MyGameController.instance.ForceLogin();
        });
    }
}
