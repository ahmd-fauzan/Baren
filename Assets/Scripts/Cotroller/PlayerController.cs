using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;

public class PlayerController : MonoBehaviour
{
    public string UserID
    {
        get; set;
    }

    private DatabaseReference dbReference;

    public DatabaseReference DbReference
    {
        get
        {
            if (dbReference == null)
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;

            return this.dbReference;
        }
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
        DatabaseManager dbManager = new DatabaseManager();

        this.history.MatchResult = matchResult;
        this.history.BattlePoint = battlePoint;

        Debug.Log("BP : " + this.history.BattlePoint);
        Debug.Log("MR : " + this.history.MatchResult);
        Debug.Log("MT : " + this.history.MatchType);

        StartCoroutine(dbManager.AddHistory(history, DbReference, UserID));
        //Store to database data history of current player
    }

    public void StoreHistory()
    {
        
    }
}
