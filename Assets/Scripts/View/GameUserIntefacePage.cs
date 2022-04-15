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
    Text draftTimer;

    [SerializeField]
    Text roundTimer;

    [SerializeField]
    GameObject DraftPickUI;

    [SerializeField]
    GameObject roundUI;

    [SerializeField]
    Text playerScoreText;

    [SerializeField]
    Text enemyScoreText;

    [SerializeField]
    GameObject winMesage;

    [SerializeField]
    GameObject loseMessage;

    [SerializeField]
    Text winBattlePointText;

    [SerializeField]
    Text loseBattlePointText;

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

    public void HideDraftPick()
    {
        DraftPickUI.SetActive(false);
    }

    public void ActiveRoundUI()
    {
        roundUI.SetActive(true);
    }

    public void UpdateScore(int roundResult)
    {
        if (roundResult == 2)
            playerScoreText.text = (int.Parse(playerScoreText.text) + 1).ToString();
        else
            enemyScoreText.text = (int.Parse(enemyScoreText.text) + 1).ToString();
    }

    public void UpdateTimer(int remainingTime)
    {
        int hours = remainingTime / 60;
        int minutes = remainingTime % 60;

        roundTimer.text = hours + ":" + minutes;
    }

    public void WinMessage(int battlePoint)
    {
        winMesage.SetActive(true);
        winBattlePointText.text = battlePoint.ToString();
    }

    public void LoseMessage(int battlePoint)
    {
        loseMessage.SetActive(true);
        loseBattlePointText.text = "-" + battlePoint.ToString();
    }
}
