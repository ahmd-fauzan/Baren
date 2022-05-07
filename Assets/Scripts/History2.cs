using Firebase;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;

public class History2 : ScriptableObject
{
    public string historyID;
    public int battlePoint;
    public int matchResult;
    public int matchType;

    public History2(string historyID, int battlePoint, int matchResult, int matchType)
    {
        this.historyID = historyID;
        this.battlePoint = battlePoint;
        this.matchResult = matchResult;
        this.matchType = matchType;
    }

}
