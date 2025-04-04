using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum LeaderboardType
{
    dateRush,
    grandPrix
}

public class UILeaderboardPanel : MonoBehaviour
{
    public LeaderboardType leaderboard_Type;
    public Panel_Leaderboard bestScore_Leaderboard;
    public Panel_Leaderboard totalScore_Leaderboard;
    public RectTransform imgBtn_Toggle;
    public TextMeshProUGUI nameLeaderboard, textDateRushLeaderboard, textGrandPrixLeaderboard;
    public RectTransform bestScore_Leaderboard_RectTransform;

    void Awake()
    {
        // Btn_leaderboard(0);
    }

    public void ToggleOnStart() // set ui as Date rush
    {
        leaderboard_Type = LeaderboardType.grandPrix;
        BtnToggleLeaderboard();
    }

    public void BtnToggleLeaderboard() // on click event
    {
        if (leaderboard_Type == LeaderboardType.dateRush)
        {
            Btn_leaderboard(1);            
        }
        else // Grand prix
        {
            Btn_leaderboard(0);            
        }
    }

    private void Btn_leaderboard(int type)
    {
        leaderboard_Type = type == 0 ? LeaderboardType.dateRush : LeaderboardType.grandPrix;
        
        switch (leaderboard_Type)
        {
            case LeaderboardType.dateRush:
                imgBtn_Toggle.DOAnchorPosX(-14, 0.2f);
                nameLeaderboard.text = "DATE RUSH LEADERBOARD";
                textDateRushLeaderboard.DOFade(1, 0.2f);
                textGrandPrixLeaderboard.DOFade(0.05f, 0.2f);

                // totalScore_Leaderboard.gameObject.SetActive(true);
                bestScore_Leaderboard_RectTransform.DOAnchorPosX(-175, 0.2f).SetEase(Ease.InOutExpo).OnComplete(() =>
                {
                    totalScore_Leaderboard.gameObject.SetActive(true);
                });

                bestScore_Leaderboard.LoadLeaderboardData(leaderboard_Type);
                totalScore_Leaderboard.LoadLeaderboardData(leaderboard_Type);
                break;
            case LeaderboardType.grandPrix:
                imgBtn_Toggle.DOAnchorPosX(14, 0.2f);
                nameLeaderboard.text = "GRAND PRIX LEADERBOARD";
                textDateRushLeaderboard.DOFade(0.05f, 0.2f);
                textGrandPrixLeaderboard.DOFade(1, 0.2f);

                totalScore_Leaderboard.gameObject.SetActive(false);
                bestScore_Leaderboard_RectTransform.DOAnchorPosX(0, 0.2f).SetEase(Ease.InOutExpo);

                bestScore_Leaderboard.LoadLeaderboardData(leaderboard_Type);
                break;
        }
    }

    public void Btn_HideLeaderboard()
    {
        gameObject.SetActive(false);
    }
}
