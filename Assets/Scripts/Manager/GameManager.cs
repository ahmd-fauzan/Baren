using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Firebase.Database;

public class GameManager : MonoBehaviourPunCallbacks
{
    GameUserIntefacePage gamePage;

    #region DraftPick
    //Selected Character From DraftPick
    [SerializeField]
    private List<Character> characterSelected;

    [SerializeField]
    private int countEnemyCharacter;

    [SerializeField]
    GameObject loadingUI;

    [SerializeField]
    Slider loadingSlider;

    [SerializeField]
    Text loadingText;

    #endregion

    //Round Score
    private int myScore;
    private int enemyScore;

    //Status -> Player1 atau Player2
    private string myStatus;

    #region CONST REWARD
    private const int WINBATTLEPOINT = 10;
    private const int LOSEBATTLEPOINT = -8;
    #endregion

    #region CONST ROUNDRESULT
    private const int ENEMYLEFT = -1;
    private const int DRAWROUND = 0;
    private const int LOSEROUND = 1;
    private const int WINROUND = 2;
    #endregion

    public string UserID
    {
        get; set;
    }

    private DatabaseReference dbReference;

    public DatabaseReference DbReference
    {
        get
        {
            if(dbReference == null)
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;

            return this.dbReference;
        }
    }

    public List<Character> CharacterSelected {
        get
        {
            return characterSelected;
        }
        set
        {
            characterSelected = value;
        }
    }

    public int MyScore
    {
        get
        {
            return myScore;
        }
        set
        {
            myScore = value;
        }
    }

    public int EnemyScore
    {
        get
        {
            return enemyScore;
        }
        set
        {
            enemyScore = value;
        }
    }

    public string MyStatus
    {
        get
        {
            return myStatus;
        }
        set
        {
            myStatus = value;
        }
    }

    public int CountEnemyCharacter
    {
        get
        {
            return countEnemyCharacter;
        }
        set
        {
            countEnemyCharacter = value;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        myStatus = GetStatus();
    }

    public string GetStatus()
    {
        if (PhotonNetwork.PlayerList[0] == PhotonNetwork.LocalPlayer)
        {
            return "Player1";
        }
        else if (PhotonNetwork.PlayerList[1] == PhotonNetwork.LocalPlayer)
        {
            return "Player2";
        }
        return "";
    }

    #region Round
    public void CalculateRoundResult()
    {
        PlayerController playerController = GameObject.Find("PlayerController").GetComponent<PlayerController>();

        if (gamePage == null)
            gamePage = GameObject.Find("GameUserInterface").GetComponent<GameUserIntefacePage>();

        DatabaseManager dbManager = new DatabaseManager();

        if (MyScore == 3)
        {
            playerController.UpdateHistory(1, WINBATTLEPOINT);

            gamePage.WinMessage(WINBATTLEPOINT);
        }

        else if (EnemyScore == 3)
        {
            playerController.UpdateHistory(1, -LOSEBATTLEPOINT);
            gamePage.LoseMessage(-LOSEBATTLEPOINT);
        }
    }

    public void UpdateScore(int roundResult)
    {
        if(gamePage == null)
            gamePage = GameObject.Find("GameUserInterface").GetComponent<GameUserIntefacePage>();

        switch (roundResult)
        {
            case LOSEROUND:
                EnemyScore++;
                break;

            case WINROUND:
                MyScore++;
                break;
        }
        
        gamePage.UpdateScore(roundResult);

    }

    public bool IsGameFinished()
    {
        if (MyScore == 3 || EnemyScore == 3 || (MyScore + EnemyScore == 5))
            return true;
        return false;
    }

    #endregion

    #region Load Scene
    public IEnumerator LoadScene(string sceneName)
    {
        loadingUI.SetActive(true);

        PhotonNetwork.LoadLevel(sceneName);
        int titik = 1;
        string text = "Loading.";
        while(PhotonNetwork.LevelLoadingProgress < 1)
        {
            if(titik <= 3)
            {
                loadingText.text = text;
                text = text + ".";
                titik++;
            }
            if(titik == 3)
            {
                text = "Loading.";
            }

            loadingSlider.value = PhotonNetwork.LevelLoadingProgress;

            Debug.Log("Loading Level : " + (PhotonNetwork.LevelLoadingProgress * 100) + "%");
            yield return new WaitForEndOfFrame();
        }
    }
    #endregion
}
