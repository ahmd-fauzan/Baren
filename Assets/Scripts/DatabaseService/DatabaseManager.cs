using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


public class DatabaseManager : ScriptableObject
{
    DatabaseManager instance;

    DatabaseReference reference;

    public DatabaseManager GetInstance()
    {
        if (instance == null)
        {
            reference = FirebaseDatabase.DefaultInstance.RootReference;

            instance = this;
        }

        return instance;
    }

    public DatabaseReference Reference
    {
        get
        {
            return this.reference;
        }
    }

    public async Task UpdatePlayerInfo(string userId, PlayerInfo pInfo)
    {
        string json = JsonUtility.ToJson(pInfo);
        
        await reference.Child("Players").Child(userId).Child("PlayerInfo").SetRawJsonValueAsync(json);

    }

    public async Task<bool> UserDataExist(string userId)
    {
        Debug.Log("User Data Exist Run : " + userId);
        Debug.Log("Reference : " + reference);

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

    public async Task<bool> AddHistory(History history, long index,  string userID)
    {
        await GetLastIndex(userID).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                long currIndex;

                if (index == -1)
                    currIndex = task.Result;
                else
                    currIndex = index;
                
                CreateHistory(history, currIndex, userID).ContinueWithOnMainThread(task =>
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

    public async Task<long> GetLastIndex(string userId)
    {
        DataSnapshot snapshot = await reference.Child("Players").Child(userId).Child("History").GetValueAsync();

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
            history.HistoryID = "HS0" + (lastIndex + 1);
        else
            history.HistoryID = "HS" + (lastIndex + 1);

        string json = JsonUtility.ToJson(history);

        await reference.Child("Players").Child(userID).Child("History").Child((lastIndex).ToString()).SetRawJsonValueAsync(json);

        return true;
    }

    public void UpdateSignedIn(bool status, string userID)
    {
        Dictionary<string, bool> signedIn = new Dictionary<string, bool>();

        signedIn.Add("SignedIn", status);

        reference.Child("Players").Child(userID).Child("Auth").SetValueAsync(signedIn);
    }

    public async Task<bool> GetSignedIn(string userID)
    {
        DataSnapshot snapshot = await reference.Child("Players").Child(userID).Child("Auth").Child("SignedIn").GetValueAsync();

        Debug.Log("Snaphost SignedIn : " + snapshot);
        if(snapshot != null && snapshot.Value != null)
        {
            Debug.Log("SignedIn : " + snapshot.Value);

            return Boolean.Parse(snapshot.Value.ToString());
        }

        return false;
    }

    public string GetUserKey()
    {
        var regexItem = new Regex("^[a-zA-Z0-9 ]*$");

        string key1 = reference.Child("Players").Push().Key;
        string key2 = reference.Child("Players").Push().Key;
        string key = "";
        foreach(char c in key1)
        {
            if (regexItem.IsMatch(c.ToString()) && key.Length < 28){
                key += c;
            }
        }

        foreach (char c in key2)
        {
            if (regexItem.IsMatch(c.ToString()) && key.Length < 28)
            {
                key += c;
            }
        }

        return key;
    } 
}
