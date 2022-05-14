using Firebase;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfo : ScriptableObject
{
    public string username;
    public int battlePoint;

    public PlayerInfo() { }
    public PlayerInfo(string username, int battlePoint)
    {
        this.username = username;
        this.battlePoint = battlePoint;
    }

   public string Username
    {
        get
        {
            return this.username;
        }
        set
        {
            this.username = value;
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
}
