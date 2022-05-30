using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Firebase.Extensions;
using System.Text.RegularExpressions;


public class MatchMakingPage : MonoBehaviour
{
    [SerializeField]
    private GameObject roomCodeIF;

    [SerializeField]
    private InputField roomNameIF;

    [SerializeField]
    private Toggle privateToggle;

    [SerializeField]
    private GameObject reconnectScreen;

    [SerializeField]
    Text KoderoomValidTXT;

    private const int PUBLICMATCH = 1;
    private const int PRIVATEMATCH = 0;

    void Start()
    {

        //userIDText.text = pController.UserID;
    }

    public void CreateRoom()
    {
        PlayerController pController = PlayerController.GetInstance();

        PlayerInfo pInfo = pController.PlayerInfo;

        if (privateToggle.isOn)
        {
            MatchMakingManager.CreateRoom(roomCodeIF.GetComponent<InputField>().text, PRIVATEMATCH);
        }
        else
        {
            MatchMakingManager.CreateRoom(pInfo.Username + "#" + pInfo.BattlePoint, PUBLICMATCH);
        }
    }

    public void JoinRoom()
    {
        var regexItem = new Regex("^[0-9 ]*$");

        if (regexItem.IsMatch(roomNameIF.text) && roomNameIF.text.Length == 6)
        {
            Debug.Log("kode room sesuai");
            MatchMakingManager manager = GameObject.Find("MatchManager").GetComponent<MatchMakingManager>().GetInstance();
            manager.JoinRoom(roomNameIF.text);
        }

        else
        {
            Debug.Log("Kode room tidak sesuai");
            KoderoomValidTXT.gameObject.SetActive(true);

        }

 
    }

    public void SetPrivateToggle()
    {
        roomCodeIF.SetActive(privateToggle.isOn);
    }

    public void ActiveReconnectSreen(bool isActive)
    {
        reconnectScreen.SetActive(isActive);
    }
}
