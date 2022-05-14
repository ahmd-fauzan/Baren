using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class RoomListItem : MonoBehaviour
{
    [SerializeField]
    Text text;

    RoomInfo info;

    public void Setup(RoomInfo _info)
    {
        info = _info;
        text.text = _info.Name;
    }

    public void OnClick()
    {
        Debug.Log("info.Name" + info.Name );
        MatchMakingManager manager = GameObject.Find("MatchManager").GetComponent<MatchMakingManager>();
        manager.JoinRoom(info.Name);
        
    }
}
