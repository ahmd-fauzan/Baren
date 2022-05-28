using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class History : ScriptableObject
{
    public string historyID;
    public int battlePoint;
    public int matchResult;
    public int matchType;

    public History(string historyID, int battlePoint, int matchResult, int matchType)
    {
        this.historyID = historyID;
        this.battlePoint = battlePoint;
        this.matchResult = matchResult;
        this.matchType = matchType;
    }


    public string HistoryID
    {
        get
        {
            return this.historyID;
        }

        set
        {
            this.historyID = value;
        }
    }
    public int BattlePoint
    {
        get
        {
            return this.battlePoint;
        }

        set
        {
            this.battlePoint = value;
        }
    }

    public int MatchResult
    {
        get
        {
            return this.matchResult;
        }

        set
        {
            this.matchResult = value;
        }
    }

    public int MatchType
    {
        get
        {
            return this.matchType;
        }

        set
        {
            this.matchType = value;
        }
    }
}
