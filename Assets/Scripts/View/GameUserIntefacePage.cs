using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;
using Photon.Pun;
using UnityEngine.UI;

public class GameUserIntefacePage : MonoBehaviour
{
    [SerializeField]
    Text playerNameText;

    [SerializeField]
    private GameObject reward;

    [SerializeField]
    private GameObject loseReward;

    [SerializeField]
    Text draftTimer;

    [SerializeField]
    GameObject DraftPickUI;

    List<Character> selectedCharacters;

    private string playerType;
    public string PlayerType 
    { 
        get
        {
            return playerType;
        }
        set {
            playerType = value;
        } 
    }

    public void SetOtherPlayerName(string playerName)
    {
        playerNameText.text = playerName;
    }

    public void SetDraftTimer(int timer)
    {
        draftTimer.text = timer.ToString();
    }

    public void Reward()
    {
        reward.SetActive(true);
    }

    public void LoseReward()
    {
        loseReward.SetActive(true);
    }

    public List<Character> GetCharacters()
    {
        return selectedCharacters;
    }

    public void HideDraftPick()
    {
        DraftPickUI.SetActive(false);
    }
}
