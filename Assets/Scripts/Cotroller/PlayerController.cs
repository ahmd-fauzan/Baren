using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;

public class PlayerController : MonoBehaviour
{
    public string UserID
    {
        get; set;
    }

    History history;

    Statistic statistic;

    PlayerController instance;

    public PlayerController Instance
    {
        get
        {
            if(instance == null)
            {
                instance = this;
            }

            return instance;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

        //getdata player
        DontDestroyOnLoad(this.gameObject);
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

    public void AddHistoy(History history)
    {
        this.history = history;
    }

    public void UpdateHistory(int matchResult, int battlePoint)
    {
        DatabaseManager dbManager = ScriptableObject.CreateInstance<DatabaseManager>().GetInstance();

        this.history.MatchResult = matchResult;
        this.history.BattlePoint = battlePoint;

        Debug.Log("BP : " + this.history.BattlePoint);
        Debug.Log("MR : " + this.history.MatchResult);
        Debug.Log("MT : " + this.history.MatchType);

        dbManager.AddHistory(history, UserID).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                dbManager.GetPlayerInfo(UserID).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted)
                    {
                        PlayerInfo pInfo = task.Result;

                        pInfo.BattlePoint += battlePoint;

                        if (pInfo.BattlePoint < 0)
                            pInfo.BattlePoint = 0;

                        dbManager.UpdatePlayerInfo(UserID, pInfo).ContinueWithOnMainThread(task =>
                        {
                            if (task.IsCompleted)
                            {
                                Debug.Log("Store Completed");
                            }
                        });
                    }
                });
            }
        });
    }
}
