using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;

public class DatabaseManager : ScriptableObject
{
    DatabaseManager instance;

    DatabaseReference reference;

    public DatabaseManager GetInstance()
    {
        if (instance == null)
        {
            instance = this;

            reference = FirebaseDatabase.DefaultInstance.RootReference;
        }

        return instance;
    }

    public void CreateUser(string username, string userId)
    {
        PlayerInfo newUser = CreateInstance<PlayerInfo>();
        newUser.Username = username;
        newUser.BattlePoint = 0;

        string json = JsonUtility.ToJson(newUser);
        
        reference.Child("Players").Child(userId).Child("PlayerInfo").SetRawJsonValueAsync(json);
    }

    public async Task<bool> UpdatePlayerInfo(string userId, PlayerInfo pInfo)
    {
        string json = JsonUtility.ToJson(pInfo);
        
         await reference.Child("Players").Child(userId).Child("PlayerInfo").SetRawJsonValueAsync(json);

        return true;
    }

    public async Task<bool> UserDataExist(string userId)
    {
        DataSnapshot snapshot = await reference.Child("Players").Child(userId).GetValueAsync();

        if(snapshot.Value == null)
        {
            return false;
        }

        return true;
    }

    public async Task<bool> UsernameExist(string username)
    {
        DataSnapshot snapshot = await reference.Child("Players").GetValueAsync();

        foreach(DataSnapshot data in snapshot.Children.Reverse<DataSnapshot>())
        {
            if ((string)data.Child("PlayerInfo").Child("username").Value == username)
                return true;
        }

        return false;
    }
     
    public async Task<PlayerInfo> GetPlayerInfo(string userID)
    {
        PlayerInfo pInfo = CreateInstance<PlayerInfo>();

        var playerInfoRef = reference.Child("Players").Child(userID).Child("PlayerInfo");

        DataSnapshot snapshot = await playerInfoRef.GetValueAsync();

        pInfo.Username = snapshot.Child("username").Value.ToString();
        pInfo.BattlePoint = int.Parse(snapshot.Child("battlePoint").Value.ToString());

        return pInfo;
    }

    public IEnumerator LoadPlayers(string userID)
    {
        Debug.Log("IEstart get statistci");

        var historyReference = reference.Child("Players").Child(userID).Child("History");

        var DBTask = historyReference.GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"failed to register task with{DBTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;

            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {
                /*     Debug.Log("else load player foreach " + childSnapshot);
                     Debug.Log("else load player foreach2 " + childSnapshot.Child("name"));*/

                String HistoryID = childSnapshot.Child("HistoryID").Value.ToString();
                int battlePoint = int.Parse(childSnapshot.Child("battlePoint").Value.ToString());
                int matchResult = int.Parse(childSnapshot.Child("matchResult").Value.ToString());
                int matchStatus = int.Parse(childSnapshot.Child("matchStatus").Value.ToString());

                Debug.Log("History ID: " + HistoryID + "---------------------");
                Debug.Log("battlePoint: " + battlePoint);
                Debug.Log("matchResult: " + matchResult);
                Debug.Log("matchStatus: " + matchStatus);

                /*                Debug.Log("end else load player foreach");
                */
            }
        }

    }


    public async Task<Statistic> GetStatistic(string userID)
    {
        Statistic statistic = CreateInstance<Statistic>();

        var historyReference = reference.Child("Players").Child(userID).Child("History");

        DataSnapshot snapshot = await historyReference.GetValueAsync();

        if (false)
        {
            //Debug.LogWarning(message: $"failed to register task with{DBTask.Exception}");
        }
        else
        {
            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {        
                int matchResult = int.Parse(childSnapshot.Child("matchResult").Value.ToString());

                if (matchResult == 1)
                    statistic.Win++;
                else if (matchResult == 0)
                    statistic.Draw++;
                else if (matchResult == -1)
                    statistic.Lose++;           
            }
        }

        return statistic;
    }

    public async Task<List<PlayerInfo>> GetLeaderboard()
    {
        List<PlayerInfo> pInfoList = new List<PlayerInfo>();

        DataSnapshot snapshot = await reference.Child("Players").GetValueAsync();

        foreach (DataSnapshot data in snapshot.Children.Reverse<DataSnapshot>())
        {
            PlayerInfo pInfo = CreateInstance<PlayerInfo>();

            pInfo.Username = data.Child("PlayerInfo").Child("username").Value.ToString();
            pInfo.BattlePoint = int.Parse(data.Child("PlayerInfo").Child("battlePoint").Value.ToString());

            if (pInfo != null)
                pInfoList.Add(pInfo);
        }

        return pInfoList;
    }

    public async Task<bool> AddHistory(History history,  string userID)
    {
        await GetLastIndex().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                long index = task.Result;
                
                CreateHistory(history, index, userID).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("Success");
                    }
                });
            }
        });

        return true;
    }

    private async Task<long> GetLastIndex()
    {
        DataSnapshot snapshot = await reference.Child("Players").Child("History").GetValueAsync();

        if(snapshot != null)
        {
            long lenght = snapshot.ChildrenCount;

            return lenght;
        }

        return -1;
    }

    public async Task<bool> CreateHistory(History history, long lastIndex, string userID)
    {
        if (lastIndex < 10)
            history.HistoryID = "HS0" + lastIndex + 1;
        else
            history.HistoryID = "HS" + lastIndex + 1;

        string json = JsonUtility.ToJson(history);

        await reference.Child("Players").Child(userID).Child("History").Child((lastIndex).ToString()).SetRawJsonValueAsync(json);

        return true;
    }
}
