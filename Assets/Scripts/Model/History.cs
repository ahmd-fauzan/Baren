using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class History : ScriptableObject
{
    private int battlePoint;
    private int matchResult;
    private int matchType;

    public History(int matchType)
    {
        this.matchType = matchType;
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
