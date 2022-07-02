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
    Text roundTimer;

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
    GameObject drawMessage;

    [SerializeField]
    Text winBattlePointText;

    [SerializeField]
    Text loseBattlePointText;

    [SerializeField]
    Text drawBattlePointText;

    [SerializeField]
    GameObject cardSelectionUI;

    [SerializeField]
    GameObject selectionIcon;

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

    public void ActiveRoundUI()
    {
        roundUI.SetActive(true);
    }

    public void UpdateScore(int roundResult)
    {
        if (roundResult == 2)
            playerScoreText.text = (int.Parse(playerScoreText.text) + 1).ToString();
        else if(roundResult == 1)
            enemyScoreText.text = (int.Parse(enemyScoreText.text) + 1).ToString();
    }

    public void ShowScore(int myScore, int enemyScore)
    {
        playerScoreText.text = myScore.ToString();
        enemyScoreText.text = enemyScore.ToString();
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
        loseBattlePointText.text = battlePoint.ToString();
    }

    public void DrawMessage(int battlePOint)
    {
        drawMessage.SetActive(true);
        drawBattlePointText.text = battlePOint.ToString();
    }

    public void HideCardSelection()
    {
        cardSelectionUI.SetActive(!cardSelectionUI.activeInHierarchy);
    }

    public void UpdateSelectionIcon()
    {
        RectTransform rect = selectionIcon.GetComponent<RectTransform>();

        Vector3 scale = rect.localScale;

        scale.x = -scale.x;

        rect.localScale = scale;
    }
}
