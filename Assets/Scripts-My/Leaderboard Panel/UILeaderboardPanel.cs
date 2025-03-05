using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
    public Image imgBtn_dateRush;
    public Image imgBtn_grandPrix;
    
    void Awake()
    {
        Btn_leaderboard(0);
    }

    public void Btn_leaderboard(int type)
    {
        leaderboard_Type = type == 0 ? LeaderboardType.dateRush : LeaderboardType.grandPrix;
        bestScore_Leaderboard.LoadLeaderboardData(leaderboard_Type);
        totalScore_Leaderboard.LoadLeaderboardData(leaderboard_Type);
        switch (leaderboard_Type)
        {
            case LeaderboardType.dateRush:
                imgBtn_dateRush.DOFade(1, 0.2f);
                imgBtn_grandPrix.DOFade(0, 0.2f);
                break;
            case LeaderboardType.grandPrix:
                imgBtn_dateRush.DOFade(0, 0.2f);
                imgBtn_grandPrix.DOFade(1, 0.2f);
                break;
        }
    }
}
