using Firebase.Database;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;

public class DatabaseManager : ScriptableObject
{
    public void CreateUser(DatabaseReference reference, string username, string userId)
    {
        PlayerInfo newUser = CreateInstance<PlayerInfo>();
        newUser.Username = username;
        newUser.BattlePoint = 0;

        string json = JsonUtility.ToJson(newUser);
        
        reference.Child("Players").Child(userId).Child("PlayerInfo").SetRawJsonValueAsync(json);
    }

    public async Task<bool> UsernameExist(DatabaseReference reference, string username)
    {
        DataSnapshot snapshot = await reference.Child("Players").GetValueAsync();

        foreach(DataSnapshot data in snapshot.Children.Reverse<DataSnapshot>())
        {
            if ((string)data.Child("PlayerInfo").Child("username").Value == username)
                return true;
        }

        return false;
    }

    public IEnumerator GetName(DatabaseReference reference, string userID, Action<string> onCallback)
    {

        var userNameData = reference.Child("Players").Child(userID).Child("name").GetValueAsync();

        yield return new WaitUntil(predicate: () => userNameData.IsCompleted);

        if (userNameData != null)
        {
            DataSnapshot snapshot = userNameData.Result;
            onCallback.Invoke(snapshot.Value.ToString());
        }

    }

    public IEnumerator GetGold(DatabaseReference reference, string userID, Action<int> onCallback)
    {
        var userGoldData = reference.Child("Players").Child(userID).Child("gold").GetValueAsync();

        yield return new WaitUntil(predicate: () => userGoldData.IsCompleted);

        if (userGoldData != null)
        {
            DataSnapshot snapshot = userGoldData.Result;
            onCallback.Invoke(int.Parse(snapshot.Value.ToString()));
        }
    }

     
    public async Task<PlayerInfo> GetPlayerInfo(DatabaseReference reference, string userID)
    {
        PlayerInfo pInfo = CreateInstance<PlayerInfo>();

        var playerInfoRef = reference.Child("Players").Child(userID).Child("PlayerInfo");

        DataSnapshot snapshot = await playerInfoRef.GetValueAsync();

        Debug.Log("Username : " + snapshot.Child("username").Value);
        Debug.Log("BattlePoint : " + snapshot.Child("battlePoint").Value.ToString());

        pInfo.Username = snapshot.Child("username").Value.ToString();
        pInfo.BattlePoint = int.Parse(snapshot.Child("battlePoint").Value.ToString());
        return pInfo;
    }

    public void getListPlayers()
    {
        //StartCoroutine(LoadPlayers());
    }

    public IEnumerator LoadPlayers(DatabaseReference reference, string userID)
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


    public async Task<Statistic> GetStatistic(DatabaseReference reference, string userID)
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

    public IEnumerator AddHistory(History history, DatabaseReference reference, string userID)
    {
        var historyReference = reference.Child("Players").Child(userID).Child("History");

        var DBTask = historyReference.GetValueAsync();

        int lastindex = 0;

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"failed to register task with{DBTask.Exception}");
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;

            int Historylength = snapshot.Children.Reverse<DataSnapshot>().Count();
        
            lastindex = Historylength - 1;
            Debug.Log("last index in load last index: " + lastindex );
            CreateHistory(history, lastindex, reference, userID);

        }
    }

    public void CreateHistory(History history, int lastIndex, DatabaseReference reference, string userID)
    {
        if(lastIndex + 1 < 10)
            history.HistoryID = "HS0" + lastIndex + 1;
        else
            history.HistoryID = "HS" + lastIndex + 1;


        string json = JsonUtility.ToJson(history);

        reference.Child("Players").Child(userID).Child("History").Child((lastIndex+1).ToString()).SetRawJsonValueAsync(json);

        Debug.Log("Create History");
    }


    /*public void setBattlePointAndUsernameTXT()
    {
        StartCoroutine(GetBattlePoint((int battlePoint) =>
        {
            Debug.Log("battlePointINfo: " + battlePoint);

            //settext batltepoint
        }));


        StartCoroutine(GetUsername((string userName) =>
        {
            Debug.Log("userName Infor: " + userName);

            //settext batltepoint
        }));

    }

    public IEnumerator GetBattlePoint(Action<int> onCallback)
    {
        var battlePointData = dbReference.Child("Players").Child(userID).Child("PlayerInfo").Child("battlePoint").GetValueAsync();

        yield return new WaitUntil(predicate: () => battlePointData.IsCompleted);

        if (battlePointData != null)
        {
            DataSnapshot snapshot = battlePointData.Result;
            onCallback.Invoke(int.Parse(snapshot.Value.ToString()));
        }

    }
    public IEnumerator GetUsername(Action<string> onCallback)
    {
        var userNameData = dbReference.Child("Players").Child(userID).Child("PlayerInfo").Child("username").GetValueAsync();

        yield return new WaitUntil(predicate: () => userNameData.IsCompleted);

        if (userNameData != null)
        {
            DataSnapshot snapshot = userNameData.Result;
            onCallback.Invoke(snapshot.Value.ToString());
        }

    }

    public void setUserIDTXT()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }*/

   
}
