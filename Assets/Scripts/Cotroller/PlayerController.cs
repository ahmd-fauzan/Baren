using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using System.IO;

public class PlayerController
{
    private PlayerInfo playerInfo;

    History history;

    Statistic statistic;

    private List<PlayerInfo> listPlayer;

    HistoryList listHistory = new HistoryList();

    private const string SAVE_FILE_PLAYER = "user.sav";
    private const string SAVE_FILE_HISTORY = "hist.sav";

    private const string keyWord = "5527080";

    public string CurrentUserId
    {
        get; set;
    }

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
    
    public float GetWinRate(int matchResult)
    {
        if (Statistic == null)
        {
            Statistic = ScriptableObject.CreateInstance<Statistic>();
            Statistic.Win = 0;
            Statistic.Draw = 0;
            Statistic.Lose = 0;
        }

        float totalWin = Statistic.Win;
        if (matchResult == 1)
            totalWin ++;

        float totalMatch = Statistic.Win + Statistic.Lose;
        if (matchResult != 0)
            totalMatch ++;


        Debug.Log("Calculate Win Rate : " + totalWin / totalMatch);
        return totalWin / totalMatch;
    }

    public void UpdateHistory(int matchResult, int battlePoint, string UserID)
    {

        //Debug.Log("History : " + this.history.MatchType);

        this.history.MatchResult = matchResult;
        this.history.BattlePoint = battlePoint;

        Debug.Log("BP : " + this.history.BattlePoint);
        Debug.Log("MR : " + this.history.MatchResult);
        Debug.Log("MT : " + this.history.MatchType);

        UpdateBattlePoint(battlePoint);
        PlayerInfo.WinRate = GetWinRate(matchResult);
        Debug.Log("Update Player Info : " + PlayerInfo.WinRate);

        if (CheckGuestPlayer())
        {
            UpdateGuestPlayerInfo(PlayerInfo);

            SaveListHistory(this.history);
        }
        else
        {

            UpdateUserData(UserID, PlayerInfo, this.history);
        }
        
    }

    public void UpdateUserData(string UserID, PlayerInfo pInfo, History history)
    {
        if (pInfo.BattlePoint < 0)
            pInfo.BattlePoint = 0;

        DatabaseManager dbManager = ScriptableObject.CreateInstance<DatabaseManager>().GetInstance();

        dbManager.UpdatePlayerInfo(UserID, pInfo).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Store Player Completed");
            }
        });

        dbManager.AddHistory(history, -1, UserID).ContinueWithOnMainThread(task =>
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
        if (snapshot == null)
            return null;

        PlayerInfo pInfo = ScriptableObject.CreateInstance<PlayerInfo>();

        pInfo.Username = snapshot.Child("username").Value.ToString();
        pInfo.BattlePoint = int.Parse(snapshot.Child("battlePoint").Value.ToString());
        pInfo.WinRate = float.Parse(snapshot.Child("winRate").Value.ToString());

        return pInfo;

    }

    public Statistic GetStatistic(DataSnapshot snapshot)
    {
        History history = new History();

        history.HistoryID = snapshot.Child("historyID").Value.ToString();
        history.MatchType = int.Parse(snapshot.Child("matchType").Value.ToString());
        history.MatchResult = int.Parse(snapshot.Child("matchResult").Value.ToString());
        history.BattlePoint = int.Parse(snapshot.Child("battlePoint").Value.ToString());

        if(this.statistic == null)
            this.statistic = ScriptableObject.CreateInstance<Statistic>();

        switch (history.matchResult)
        {
            case 1:
                Statistic.Win++;
                break;
            case 0:
                Statistic.Draw++;
                break;
            case -1:
                Statistic.Lose++;
                break;
        }

        return statistic;
    }

    public Statistic GetGuestStatistic(List<History> historyList)
    {
        statistic = ScriptableObject.CreateInstance<Statistic>();

        foreach(History his in historyList)
        {
            switch (his.matchResult)
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
        }

        return statistic;
    }

    public void CreateGuestPlayerInfo()
    {
        PlayerInfo pInfo = ScriptableObject.CreateInstance<PlayerInfo>();

        pInfo.Username = "Guest";
        pInfo.BattlePoint = 0;
        pInfo.WinRate = 1;

        string json = EncryptDecrypt(JsonUtility.ToJson(pInfo));

        string filename = Path.Combine(Application.persistentDataPath + SAVE_FILE_PLAYER);

        if(!File.Exists(filename))
            File.WriteAllText(filename, json);

    }

    public void UpdateGuestPlayerInfo(PlayerInfo pInfo)
    {
        if (pInfo.BattlePoint < 0)
            pInfo.BattlePoint = 0;

        string json = EncryptDecrypt(JsonUtility.ToJson(pInfo));

        string filename = Path.Combine(Application.persistentDataPath + SAVE_FILE_PLAYER);

        File.WriteAllText(filename, json);
    }

    public bool CheckGuestPlayer()
    {
        string filename = Path.Combine(Application.persistentDataPath + SAVE_FILE_PLAYER);

        if (File.Exists(filename))
            return true;

        return false;
    }

    public void DeleteFileGuest()
    {
        string filenamePlayerInfo = Path.Combine(Application.persistentDataPath + SAVE_FILE_PLAYER);

        string filenameHistory = Path.Combine(Application.persistentDataPath + SAVE_FILE_HISTORY);

        File.Delete(filenamePlayerInfo);

        File.Delete(filenameHistory);
    }

    public void AddGuestHistory()
    {
        string json = EncryptDecrypt(JsonUtility.ToJson(this.history));

        string filename = Path.Combine(Application.persistentDataPath + SAVE_FILE_HISTORY);

        File.AppendAllText(filename, json);
    }

    public PlayerInfo LoadGuestPlayerInfo()
    {
        string filename = Path.Combine(Application.persistentDataPath + SAVE_FILE_PLAYER);

        if (File.Exists(filename))
        {
            string json = EncryptDecrypt(File.ReadAllText(filename));

            PlayerInfo pInfo = ScriptableObject.CreateInstance<PlayerInfo>();

            JsonUtility.FromJsonOverwrite(json, pInfo);

            PlayerInfo = pInfo;

            return pInfo;
        }

        return null;
    }

    public void SaveListHistory(History newHistory)
    {
        listHistory.histories = LoadGuestHistory();

        if(listHistory.histories == null)
        {
            listHistory.histories = new List<History>();
        }

        listHistory.histories.Add(newHistory);

        string json = EncryptDecrypt(JsonUtility.ToJson(listHistory));

        Debug.Log("JSON : " + json);

        string filename = Path.Combine(Application.persistentDataPath + SAVE_FILE_HISTORY);

        File.WriteAllText(filename, json);
    }

    [System.Serializable]
    public class HistoryList
    {
        public List<History> histories;
    }


    public List<History> LoadGuestHistory()
    {
        string filename = Path.Combine(Application.persistentDataPath + SAVE_FILE_HISTORY);


        if (File.Exists(filename))
        {
            string json = EncryptDecrypt(File.ReadAllText(filename));

            listHistory = JsonUtility.FromJson<HistoryList>(json);

            return listHistory.histories;

            /*foreach(History h in listHistory.histories)
            {
                Debug.Log("HistoryText : " + h.BattlePoint);
            }*/

        }

        return null;
    }

    private string EncryptDecrypt(string data)
    {
        string result = "";

        for(int i = 0; i < data.Length; i++)
        {
            result += (char) (data[i] ^ keyWord[i % keyWord.Length]);
        }

        return result;
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
