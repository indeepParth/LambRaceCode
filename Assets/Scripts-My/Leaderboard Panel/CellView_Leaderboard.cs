using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CellView_Leaderboard : EnhancedScrollerCellView
{
    public TextMeshProUGUI text_Rank;
    public TextMeshProUGUI text_Name;
    public TextMeshProUGUI text_Heart;
    public void SetData(DataLeaderBoard data)
    {
        text_Rank.text = data.rank + ".";
        text_Name.text = data.name;
        text_Heart.text = data.heart;
    }
}
