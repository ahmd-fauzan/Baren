using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Firebase.Extensions;

public class MatchMakingPage : MonoBehaviour
{
    [SerializeField]
    private GameObject roomCodeIF;

    [SerializeField]
    private InputField roomNameIF;

    [SerializeField]
    private Toggle privateToggle;

    private const int PUBLICMATCH = 1;
    private const int PRIVATEMATCH = 0;

    void Start()
    {

        //userIDText.text = pController.UserID;
    }

    public void CreateRoom()
    {
        PlayerController pController = GameObject.Find("PlayerController").GetComponent<PlayerController>().Instance;
        DatabaseManager dbManager = ScriptableObject.CreateInstance<DatabaseManager>();

        dbManager.GetPlayerInfo(pController.DbReference, pController.UserID).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                PlayerInfo pInfo = task.Result;

                Debug.Log("Toogle : " + privateToggle.isOn);

                if (privateToggle.isOn)
                {
                    MatchMakingManager.CreateRoom(roomCodeIF.GetComponent<InputField>().text, PRIVATEMATCH);
                }
                else
                {
                    MatchMakingManager.CreateRoom(pInfo.Username + "#" + pInfo.BattlePoint, PUBLICMATCH);
                }
            }
        });

        

        //manager.CreateRoom()
    }

    public void JoinRoom()
    {
        MatchMakingManager manager = GameObject.Find("MatchManager").GetComponent<MatchMakingManager>().GetInstance();
        manager.JoinRoom(roomNameIF.text);
    }

    public void SetPrivateToggle()
    {
        roomCodeIF.SetActive(privateToggle.isOn);
    }
}
