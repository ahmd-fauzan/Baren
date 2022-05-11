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

    // Start is called before the first frame update
    void Start()
    {
        //getdata player
        DontDestroyOnLoad(this.gameObject);
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

        StartCoroutine(dbManager.AddHistory(history, DbReference, UserID));
        //Store to database data history of current player
    }

    public void StoreHistory()
    {
        
    }
}
