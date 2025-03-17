using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.Json;
using Unity.VisualScripting;
using System.Linq;

public class PlayFabLogin : MonoBehaviour
{
    public delegate void GSPlayerDataList(List<MyPlayerDetails> players);

    public delegate void GSPlayerDatails(MyPlayerDetails player);

    public delegate void GSHeartDatails(int heart);

    public delegate void GSBool(bool success);

    public void Start()
    {
        // if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
        // {
        //     /*
        //     Please change the titleId below to your own titleId from PlayFab Game Manager.
        //     If you have already set the value in the Editor Extensions, this can be skipped.
        //     */
        //     PlayFabSettings.staticSettings.TitleId = "C25EB";
        // }
        // var request = new LoginWithCustomIDRequest { CustomId = "ParthUnity", CreateAccount = true };
        // PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);

        // if (!string.IsNullOrEmpty(GetStoredWalletAddress()))
        // {
        //     LoginWithPlayfab();
        // }
        // else
        // {
        //     MyGameController.instance.UILoginPanel.gameObject.SetActive(true);
        // }
    }

    public void LoginWithPlayfab()
    {
        var request = new LoginWithCustomIDRequest { CustomId = GetStoredWalletAddress(), CreateAccount = true };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        MyGameController.instance.UILoginPanel.gameObject.SetActive(false);
        // Debug.Log("Congratulations, you made your first successful API call!");
        if (result != null) // && !result.NewlyCreated)
        {
            UpdatePlayerDetails(GetStoredPlayerName(), (respoce) =>
                {
                    if (!result.NewlyCreated)
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
                });
        }
        else
        {
            MyGameController.instance.UpdateHeartPoint(0);
        }
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogWarning("Something went wrong with Login address.  :(");
        MyGameController.instance.Popup_ShowMessageOnly.Init("Something went wrong with Login \n try again later.");
        MyGameController.instance.Popup_ShowMessageOnly.gameObject.SetActive(true);
        MyGameController.instance.UILoginPanel.gameObject.SetActive(true);
    }

    ////
    /// <summary>
    /// 
    /// </summary>
    /// 

    public string GetStoredWalletAddress()
    {
        return PlayerPrefs.GetString(Utility.KEY_WALLET_ID);
    }

    public void StoreWalletAddress(string wallet)
    {
        PlayerPrefs.SetString(Utility.KEY_WALLET_ID, wallet);
    }

    public string GetStoredPlayerName()
    {
        return PlayerPrefs.GetString(Utility.KEY_PLAYERNAME);
    }

    public void StorePlayerName(string name)
    {
        PlayerPrefs.SetString(Utility.KEY_PLAYERNAME, name);
    }

    public void UpdatePlayerDetails(string newName, GSBool callback)
    {
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = newName //PlayerPrefs.GetString(NMPhoton.KEY_PLAYERNAME)
        }, (res) =>
        {
            StorePlayerName(newName);
            callback?.Invoke(true);
        }, (err) =>
        {
            Debug.LogError("UpdatePlayerDetails failed.");
            callback?.Invoke(false);
        });
    }

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
    public void SubmitBestHeartOnDropPessanger(int heart) //, GSHeartDatails callback) // for Grand prix mode only
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "BestHeartOnDropPessanger",
            FunctionParameter = new { bestHeart = heart.ToString() }
        }, (result) =>
        {
            // JsonObject jsonResult = (JsonObject)result.FunctionResult;
            // object heart;
            // jsonResult.TryGetValue("lapCounter", out heart);
            // int.TryParse(heart.ToString(), out int w);
            // callback?.Invoke(w);
        }, (error) =>
        {
            Debug.LogError("LapTime On GameOver failed.");
            // Debug.LogError(error.GenerateErrorReport());
            // callback?.Invoke(0);
        });
    }
    public void SubmitLapsTimeOnGameOver(int _lapTime) //, GSHeartDatails callback) // for Grand prix mode only
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "LapTimeOnGameOver",
            FunctionParameter = new { lapTime = _lapTime.ToString() }
        }, (result) =>
        {
            // JsonObject jsonResult = (JsonObject)result.FunctionResult;
            // object heart;
            // jsonResult.TryGetValue("lapCounter", out heart);
            // int.TryParse(heart.ToString(), out int w);
            // callback?.Invoke(w);
        }, (error) =>
        {
            Debug.LogError("LapTime On GameOver failed.");
            // Debug.LogError(error.GenerateErrorReport());
            // callback?.Invoke(0);
        });
    }

    // List<MyPlayerDetails> leaderloardItems = new List<MyPlayerDetails>();
    string rank;
    string playerName;
    string wins;
    public void FetchLeaderboard_DateRush_TotalScore(GSPlayerDataList callback)
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
                    List<MyPlayerDetails> leaderloardItems = new List<MyPlayerDetails>();
                    leaderloardItems.Clear();
                    foreach (PlayerLeaderboardEntry entry in result.Leaderboard)
                    {
                        // rank = (entry.Position + 1).ToString();
                        // playerName = entry.DisplayName;
                        // wins = entry.StatValue.ToString();
                        leaderloardItems.Add(new MyPlayerDetails((entry.Position + 1).ToString()
                        , entry.DisplayName, entry.StatValue.ToString()));
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
    public void FetchLeaderboard_DateRush_BestScore(GSPlayerDataList callback)
    {
        PlayFabClientAPI.GetLeaderboard(
                // Request
                new GetLeaderboardRequest
                {
                    StatisticName = "BestHeart",
                    StartPosition = 0,
                    MaxResultsCount = 100
                },
                // Success
                (GetLeaderboardResult result) =>
                {
                    List<MyPlayerDetails> leaderloardItems = new List<MyPlayerDetails>();
                    leaderloardItems.Clear();
                    foreach (PlayerLeaderboardEntry entry in result.Leaderboard)
                    {
                        // rank = (entry.Position + 1).ToString();
                        // playerName = entry.DisplayName;
                        // wins = entry.StatValue.ToString();
                        leaderloardItems.Add(new MyPlayerDetails((entry.Position + 1).ToString()
                        , entry.DisplayName, entry.StatValue.ToString()));
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

    public void FetchLeaderboard_GrandPrix_TotalScore(GSPlayerDataList callback)
    {
        PlayFabClientAPI.GetLeaderboard(
                // Request
                new GetLeaderboardRequest
                {
                    StatisticName = "LapCounter",
                    StartPosition = 0,
                    MaxResultsCount = 100
                },
                // Success
                (GetLeaderboardResult result) =>
                {
                    List<MyPlayerDetails> leaderloardItems = new List<MyPlayerDetails>();
                    leaderloardItems.Clear();
                    foreach (PlayerLeaderboardEntry entry in result.Leaderboard)
                    {
                        // rank = (entry.Position + 1).ToString();
                        // playerName = entry.DisplayName;
                        // wins = entry.StatValue.ToString();
                        leaderloardItems.Add(new MyPlayerDetails((entry.Position + 1).ToString()
                        , entry.DisplayName, entry.StatValue.ToString()));
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
    public void FetchLeaderboard_GrandPrix_BestScore(GSPlayerDataList callback)
    {
        PlayFabClientAPI.GetLeaderboard(
                // Request
                new GetLeaderboardRequest
                {
                    StatisticName = "LapTime",
                    StartPosition = 0,
                    MaxResultsCount = 100
                },
                // Success
                (GetLeaderboardResult result) =>
                {
                    List<PlayerLeaderboardEntry> leaderloardItemsAsending = new List<PlayerLeaderboardEntry>();
                    leaderloardItemsAsending = result.Leaderboard.OrderBy(x => x.StatValue).ToList();

                    List<MyPlayerDetails> leaderloardItems = new List<MyPlayerDetails>();
                    leaderloardItems.Clear();
                    for (int i = 0; i < leaderloardItemsAsending.Count; i++)
                    {
                        PlayerLeaderboardEntry entry = leaderloardItemsAsending[i];
                        // rank = (entry.Position + 1).ToString();
                        // playerName = entry.DisplayName;
                        // wins = entry.StatValue.ToString();
                        leaderloardItems.Add(new MyPlayerDetails((i + 1).ToString()
                        , entry.DisplayName, entry.StatValue.ToString()));
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
