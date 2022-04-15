using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class MatchMakingPage : MonoBehaviour
{
    [SerializeField]
    private InputField roomCodeIF;

    [SerializeField]
    private InputField roomNameIF;

    [SerializeField]
    private Toggle publicToggle;

    [SerializeField]
    private Toggle privateToggle;

    private const int PUBLICMATCH = 1;
    private const int PRIVATEMATCH = 0;

    public void CreateRoom()
    {
        if (publicToggle.isOn)
            MatchMakingManager.CreateRoom(PhotonNetwork.NickName, PUBLICMATCH);

        if (privateToggle.isOn)
            MatchMakingManager.CreateRoom(roomCodeIF.text, PRIVATEMATCH);
        
        //manager.CreateRoom()
    }

    public void JoinRoom()
    {
        MatchMakingManager.JoinRoom(roomNameIF.text);
    }

    public void SetPublicToggle()
    {
        if(privateToggle.isOn)
            publicToggle.isOn = false;
    }

    public void SetPrivateToggle()
    {
        if(publicToggle.isOn)
            privateToggle.isOn = false;
    }
}
