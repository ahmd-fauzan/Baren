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
    private GameObject serverFullScreen;

    [SerializeField]
    private MatchMakingManager matchManager;

    [SerializeField]
    private GameObject roomScreen;

    [SerializeField]
    private GameObject roomCodeScreen;

    [SerializeField]
    private GameObject lobbyScreen;

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

        var regexItem = new Regex("^[0-9 ]*$");

        
        if (privateToggle.isOn)
        {
            if (regexItem.IsMatch(roomCodeIF.GetComponent<InputField>().text) && roomCodeIF.GetComponent<InputField>().text.Length == 6)
            {
                matchManager.CreateRoom(roomCodeIF.GetComponent<InputField>().text, PRIVATEMATCH);
            }
            else
            {
                ShowErrorMessage("Kode Room Tidak Valid");
            }
        }
        else
        {
            matchManager.CreateRoom(pInfo.Username + "#" + pInfo.BattlePoint, PUBLICMATCH);
        }
    }

    public void JoinRoom()
    {
        var regexItem = new Regex("^[0-9 ]*$");

        if (regexItem.IsMatch(roomNameIF.text) && roomNameIF.text.Length == 6)
        {
            matchManager.JoinRoom(roomNameIF.text);
        }

        else
        {
            Debug.Log("Kode room tidak sesuai");
            ShowErrorMessage("Kode Room Tidak Valid");
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

    public void ShowErrorMessage(string message)
    {
        StartCoroutine(ShowMessageError());
        KoderoomValidTXT.text = message;
    }

    IEnumerator ShowMessageError()
    {
        KoderoomValidTXT.gameObject.SetActive(true);

        float counter = 4f;
        while(counter > 0)
        {
            yield return new WaitForSeconds(1f);

            counter--;
        }
        KoderoomValidTXT.gameObject.SetActive(false);
    }

    public void ShowServerFullScreen()
    {
        serverFullScreen.SetActive(true);
    }

    public void ShowScreen(string screenName)
    {
        roomScreen.SetActive(screenName == "roomScreen");
        lobbyScreen.SetActive(screenName == "lobbyScreen");
    }

    public void ShowPopUp(string popUpName)
    {
        roomCodeScreen.SetActive(popUpName == "roomCodeScreen");
    }
}
