using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.Json;
using Unity.VisualScripting;

public class PlayFabLogin : MonoBehaviour
{
    public delegate void GSPlayerDataList(List<MyPlayerDetails> players);

    public delegate void GSPlayerDatails(MyPlayerDetails player);

    public delegate void GSHeartDatails(int heart);

    public void Start()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
        {
            /*
            Please change the titleId below to your own titleId from PlayFab Game Manager.
            If you have already set the value in the Editor Extensions, this can be skipped.
            */
            PlayFabSettings.staticSettings.TitleId = "C25EB";
        }
        var request = new LoginWithCustomIDRequest { CustomId = "ParthUnity", CreateAccount = true };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Congratulations, you made your first successful API call!");
        if(result != null && !result.NewlyCreated)
        {
            FetchPlayerDataFromGS((respoce) =>
            {
                Debug.Log("heart = " + respoce);
                MyGameController.instance.UpdateHeartPoint(respoce);
            });
        }
        else
        {
            MyGameController.instance.UpdateHeartPoint(0);
        }
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogWarning("Something went wrong with your first API call.  :(");
        Debug.LogError("Here's some debug information:");
        Debug.LogError(error.GenerateErrorReport());
    }

    ////
    /// <summary>
    /// 
    /// </summary>

    private static Dictionary<string, UserDataRecord> UserData;
    public void FetchPlayerDataFromGS(GSHeartDatails callback)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {
            Keys = new List<string> { "heart" }
        }, (result) =>
        {
            Debug.Log("GetUserData completed.");
            UserData = result.Data;

            int w = 0;
            int.TryParse(UserData["heart"].Value, out w);
            callback?.Invoke(w);
        }, (error) =>
        {
            Debug.LogError("GetUserData failed.");
            Debug.LogError(error.GenerateErrorReport());
            callback?.Invoke(0);
        });
    }

    public void GameWonRewardEvent(GSHeartDatails callback)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "DropPassenger"
        }, (result) =>
        {
            JsonObject jsonResult = (JsonObject)result.FunctionResult;

            object heart;
            jsonResult.TryGetValue("heart", out heart);
            int.TryParse(heart.ToString(), out int w);
            callback?.Invoke(w);
        }, (error) =>
        {
            Debug.LogError("GameWonRewardEvent failed.");
            Debug.LogError(error.GenerateErrorReport());
            callback?.Invoke(0);
        });
    }

    List<MyPlayerDetails> leaderloardItems = new List<MyPlayerDetails>();
    string rank;
    string playerName;
    string wins;
    public void FetchLeaderboardDataFromGS(GSPlayerDataList callback)
    {
        PlayFabClientAPI.GetLeaderboard(
                // Request
                new GetLeaderboardRequest
                {
                    StatisticName = "Heart",
                    StartPosition = 0,
                    MaxResultsCount = 100
                },
                // Success
                (GetLeaderboardResult result) =>
                {
                    leaderloardItems.Clear();
                    foreach (PlayerLeaderboardEntry entry in result.Leaderboard)
                    {
                        rank = (entry.Position + 1).ToString();
                        playerName = entry.DisplayName;
                        wins = entry.StatValue.ToString();
                        leaderloardItems.Add(new MyPlayerDetails(rank, playerName, wins));
                    }
                    callback?.Invoke(leaderloardItems);
                },
                // Failure
                (PlayFabError error) =>
                {
                    Debug.Log("Error Retrieving Leaderboard Data...");
                    Debug.LogError(error.GenerateErrorReport());
                    callback?.Invoke(null);
                }
                );
    }

    MyPlayerDetails playerLeaderlordDetail = new MyPlayerDetails();

    List<string> leaderloards = new List<string> { "HighestWins" };
    public void FetchPlayerLeaderboardDataFromGS(GSPlayerDatails callback)
    {
        PlayFabClientAPI.GetLeaderboardAroundPlayer(
                // Request
                new GetLeaderboardAroundPlayerRequest
                {
                    StatisticName = "High Score",
                    MaxResultsCount = 1
                },
                // Success
                (result) =>
                {
                    foreach (PlayerLeaderboardEntry entry in result.Leaderboard)
                    {
                        playerLeaderlordDetail.rank = (entry.Position + 1).ToString();
                        playerLeaderlordDetail.name = entry.DisplayName;
                        playerLeaderlordDetail.heart = entry.StatValue.ToString();
                    }
                    callback?.Invoke(playerLeaderlordDetail);
                },
                // Failure
                (PlayFabError error) =>
                {
                    Debug.Log("Error Retrieving Player Leaderboard Data...");
                    Debug.LogError(error.GenerateErrorReport());
                    callback?.Invoke(new MyPlayerDetails());
                }
                );
    }
}

public struct MyPlayerDetails
{
    public string rank;
    public string name;
    public string heart;

    public MyPlayerDetails(string _rank, string _name, string _heart)
    {
        rank = _rank;
        name = _name;
        heart = _heart;
    }

}
