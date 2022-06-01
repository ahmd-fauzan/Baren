using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;

public class PlayerController
{
    private PlayerInfo playerInfo;

    History history;

    Statistic statistic;

    private List<PlayerInfo> listPlayer;


    

    public Statistic Statistic
    {
        get
        {
            return this.statistic;
        }

        set
        {
            this.statistic = value;
        }
    }

    public PlayerInfo PlayerInfo 
    { 
        get 
        {
            return this.playerInfo;
        } 
        set
        {
            this.playerInfo = value;

        }
    }

    public List<PlayerInfo> Leaderboard
    {
        get
        {
            return this.listPlayer;
        }
        set
        {
            this.listPlayer = value;

        }
    }

    private static PlayerController instance;

    public static PlayerController GetInstance()
    {
        if (instance == null)
        {
            instance = new PlayerController();

        }

        return instance;
    }

    public History GetHistory()
    {
        return this.history;
    }

    public void UpdateBattlePoint(int battlePoint)
    {
        PlayerInfo.battlePoint += battlePoint;
    }

    public void AddHistoy(History history)
    {
        this.history = history;
    }


    public void UpdateHistory(int matchResult, int battlePoint, string UserID)
    {
        DatabaseManager dbManager = ScriptableObject.CreateInstance<DatabaseManager>().GetInstance();

        //Debug.Log("History : " + this.history.MatchType);

        this.history.MatchResult = matchResult;
        this.history.BattlePoint = battlePoint;

        Debug.Log("BP : " + this.history.BattlePoint);
        Debug.Log("MR : " + this.history.MatchResult);
        Debug.Log("MT : " + this.history.MatchType);

        UpdateBattlePoint(battlePoint);

        if (PlayerInfo.BattlePoint < 0)
            PlayerInfo.BattlePoint = 0;

        dbManager.UpdatePlayerInfo(UserID, playerInfo).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Store Player Completed");
            }
        });

        dbManager.AddHistory(history, UserID).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Store History Completed");
            }
        });
    }


    public List<PlayerInfo> UpdateLeaderboard(object sender, ChildChangedEventArgs args)
    {
        PlayerInfo pInfo = ScriptableObject.CreateInstance<PlayerInfo>();

        pInfo.Username = args.Snapshot.Child("PlayerInfo").Child("username").Value.ToString();
        pInfo.BattlePoint = int.Parse(args.Snapshot.Child("PlayerInfo").Child("battlePoint").Value.ToString());

        foreach (PlayerInfo info in this.listPlayer)
        {
            if (info.Username == pInfo.Username)
            {
                info.Username = pInfo.Username;
                info.BattlePoint = pInfo.BattlePoint;

                return this.listPlayer;
            }
        }

        return this.listPlayer;
    }

    public PlayerInfo GetUser(DataSnapshot snapshot)
    {
        PlayerInfo pInfo = ScriptableObject.CreateInstance<PlayerInfo>();

        pInfo.Username = snapshot.Child("username").Value.ToString();
        pInfo.BattlePoint = int.Parse(snapshot.Child("battlePoint").Value.ToString());

        return pInfo;

    }

    public Statistic GetStatistic(DataSnapshot snapshot)
    {
        History history = ScriptableObject.CreateInstance<History>();

        history.HistoryID = snapshot.Child("historyID").Value.ToString();
        history.MatchType = int.Parse(snapshot.Child("matchType").Value.ToString());
        history.MatchResult = int.Parse(snapshot.Child("matchResult").Value.ToString());
        history.BattlePoint = int.Parse(snapshot.Child("battlePoint").Value.ToString());

        if (statistic == null)
            statistic = ScriptableObject.CreateInstance<Statistic>();

        switch (history.matchResult)
        {
            case 1:
                statistic.Win++;
                break;
            case 0:
                statistic.Draw++;
                break;
            case -1:
                statistic.Lose++;
                break;
        }

        return statistic;
    }

    /*
    public void GetPlayerInfo()
    {
        Debug.Log("User ID" + UserID);

        Debug.Log("Load Player Data");

        dbManager.GetPlayerInfo(UserID).ContinueWithOnMainThread(task =>
        {
            Debug.Log(task.Result);
            if (task.IsCompleted)
            {
                PlayerInfo = task.Result;
                Debug.Log("Run GEtPlayerInfo in Player Controller");
            }
            if (task.IsCanceled)
            {
                Debug.Log("Canceled");
            }
            if (task.IsFaulted)
            {
                Debug.Log("FAulted" + task.Exception);
            }
        });

    } 
     
    public void GetHistory()
    {
        dbManager.GetStatistic(UserID).ContinueWithOnMainThread(task2 =>
        {
            if (task2.IsCompleted)
            {
                Statistic = task2.Result;
            }

            if (task2.IsFaulted)
            {
                Debug.Log("Error : " + task2.Exception);
            }
            Debug.Log("PlayerInfo");
        });
    }

    

    

    public void GetLeaderboard()
    {
        dbManager.GetLeaderboard().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Leaderboard = task.Result;
            }
        });
    }
    */
}
