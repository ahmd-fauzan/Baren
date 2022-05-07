using Firebase;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;

public class User : MonoBehaviour
{
    public string name;
    public int gold;

    public User(string name, int gold)
    {
        this.name = name;
        this.gold = gold;
    }

   
}
