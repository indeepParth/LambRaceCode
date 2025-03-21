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
    public TextMeshProUGUI nameLeaderboard;

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
        bestScore_Leaderboard.LoadLeaderboardData(leaderboard_Type);
        totalScore_Leaderboard.LoadLeaderboardData(leaderboard_Type);
        switch (leaderboard_Type)
        {
            case LeaderboardType.dateRush:
                imgBtn_Toggle.DOAnchorPosX(-14, 0.2f);
                nameLeaderboard.text = "DATE RUSH LEADERBOARD";
                break;
            case LeaderboardType.grandPrix:
                imgBtn_Toggle.DOAnchorPosX(14, 0.2f);
                nameLeaderboard.text = "GRAND PRIX LEADERBOARD";
                break;
        }
    }

    public void Btn_HideLeaderboard()
    {
        gameObject.SetActive(false);
    }
}
