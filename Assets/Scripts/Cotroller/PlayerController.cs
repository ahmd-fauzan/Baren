using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
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
        this.history.MatchResult = matchResult;
        this.history.BattlePoint = battlePoint;
        //Store to database data history of current player
    }
}
