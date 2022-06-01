using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardPage : MonoBehaviour
{
    [SerializeField]
    private GameObject leaderboardItem;

    [SerializeField]
    private Transform leaderboardSpawn;

    private List<GameObject> leaderboardItemList;

    private Color stPlaceColor = new Color32(255, 215, 0, 255);
    private Color ndPlaceColor = new Color32(192, 192, 192, 255);
    private Color rdPlaceColor = new Color32(205, 127, 50, 255);
    private Color thPlaceColor = new Color32(130, 111, 102, 255);

    // Start is called before the first frame update
    void Start()
    {
        leaderboardItemList = new List<GameObject>();

    }

    public void UpdateLeaderboard(List<PlayerInfo> leaderboardList)
    {
        int placePos = 0;
        foreach (GameObject go in leaderboardItemList)
        {
            Destroy(go);
        }

        foreach (PlayerInfo pInfo in leaderboardList)
        {
            Debug.Log("Leaderboard : " + pInfo.Username);

            placePos++;

            GameObject leaderboardGo = Instantiate(leaderboardItem, leaderboardSpawn);

            leaderboardItemList.Add(leaderboardGo);

            switch (placePos)
            {
                case 1:
                    leaderboardGo.transform.GetChild(0).GetComponent<Image>().color = stPlaceColor;
                    break;
                case 2:
                    leaderboardGo.transform.GetChild(0).GetComponent<Image>().color = ndPlaceColor;
                    break;
                case 3:
                    leaderboardGo.transform.GetChild(0).GetComponent<Image>().color = rdPlaceColor;
                    break;
                default:
                    leaderboardGo.transform.GetChild(0).GetComponent<Image>().color = thPlaceColor;
                    break;
            }

            leaderboardGo.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = placePos.ToString();
            leaderboardGo.transform.GetChild(1).GetComponent<Text>().text = pInfo.Username;
            leaderboardGo.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = pInfo.BattlePoint.ToString();

            if (placePos == 11)
            {
                return;
            }
        }
    }
}
