using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePage : MonoBehaviour
{
    #region Profile Menu
    [SerializeField]
    private Text profileNameText;

    [SerializeField]
    private Text profileIdText;

    [SerializeField]
    private Text winStatistic;

    [SerializeField]
    private Text loseStatistic;

    [SerializeField]
    private Text drawStatistic;
    #endregion

    #region Header
    [SerializeField]
    private Text usernameText;

    [SerializeField]
    private Text battlePointText;
    #endregion

    public void SetProfileInfo(PlayerInfo info, string userId)
    {
        profileNameText.text = info.Username;
        profileIdText.text = "#" + userId;
    }

    public void SetStatisticInfo(Statistic statistic)
    {
        winStatistic.text = statistic.Win.ToString();
        loseStatistic.text = statistic.Lose.ToString();
        drawStatistic.text = statistic.Draw.ToString();
    }

    public void SetPlayerInfo(PlayerInfo info)
    {
        usernameText.text = info.Username;
        battlePointText.text = info.BattlePoint.ToString();
    }
}
