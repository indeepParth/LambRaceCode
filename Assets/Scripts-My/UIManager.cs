
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject homePanel;
    public GameObject gamePanel;
    public GameObject gameFreePanel;
    public GameObject gameOverPanel;
    public TextMeshProUGUI countDownText;
    public TextMeshProUGUI timeBonusTextAnimAdd;
    public RectTransform img_BoostReaction;
    private bool boostReactionAnimPlaying = false;
    public RectTransform img_BoostAvailable;
    private bool boostAvailable;
    public TextMeshProUGUI heartPointText;

    private void Start()
    {
        MyGameController.instance.ResetGame();
        homePanel.SetActive(true);
        gameFreePanel.SetActive(false);
        gamePanel.SetActive(false);
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
        SceneLoader(1, () =>
        {
            MyGameController.instance.StartGame();
            gameFreePanel.SetActive(true);
            gamePanel.SetActive(false);
            homePanel.SetActive(false);
            IsBoostAvailable(false);
        });
    }

    public void BTN_MiamiCrazyTextMode()
    {
        // MyGameController.instance.freeMode = false;
        MyGameController.instance.gameMode = GameMode.DateRush;
        SceneLoader(1, () =>
        {
            MyGameController.instance.StartGame();
            gamePanel.SetActive(true);
            gameFreePanel.SetActive(false);
            homePanel.SetActive(false);
            IsBoostAvailable(false);
        });
    }

    public void Btn_HomeOnGameOver()
    {
        MyGameController.instance.gameMode = GameMode.None;
        MyGameController.instance.MySoundManager.RaceTrackSound(false);
        SceneLoader(0, () =>
        {            
            gameOverPanel.SetActive(false);
            gamePanel.SetActive(false);
            gameFreePanel.SetActive(false);
            homePanel.SetActive(true);
            IsBoostAvailable(false);
        });
    }
    //

    // IN Game UI
    public void UpdateCountDownTimer(int time)
    {
        countDownText.text = time.ToString() + "s";
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
                gameFreePanel.SetActive(true); gamePanel.SetActive(false);
            }
            else
            {
                gameFreePanel.SetActive(false); gamePanel.SetActive(true);
            }          
        });
    }    

    public void ShowGameOver()
    {
        gamePanel.SetActive(false);
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
        if(_boostAvailable && !boostAvailable)
        {
            boostAvailable = true;
            img_BoostAvailable.DOAnchorPosX(-10, 0.5f).SetEase(Ease.InOutExpo);
            MyGameController.instance.MyCarStateUI.ResetBoostWhenAvailable();
        }
        else if(!_boostAvailable)
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
    }
}
