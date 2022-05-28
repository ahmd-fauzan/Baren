using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Firebase.Database;
using Firebase.Auth;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    GameUserIntefacePage gamePage;

    #region DraftPick
    //Selected Character From DraftPick
    [SerializeField]
    private List<Character> characterSelected;

    [SerializeField]
    private int countEnemyCharacter;

    #endregion

    [SerializeField]
    private string manualUser;

    //Status -> Player1 atau Player2
    private string myStatus;

    #region CONST REWARD
    private const int WINBATTLEPOINT = 10;
    private const int LOSEBATTLEPOINT = -8;
    private const int DRAWBATTLEPOINT = 2;
    #endregion

    #region CONST MATCHRESULT
    private const int WINMATCH = 1;
    private const int DRAWMATCH = 0;
    private const int LOSEMATCH = -1;
    #endregion

    #region CONST ROUNDRESULT
    private const int ENEMYLEFT = -1;
    private const int DRAWROUND = 0;
    private const int LOSEROUND = 1;
    private const int WINROUND = 2;
    #endregion

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

    private List<int> listRoundResult;

    private static GameManager instance;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        if (instance == null)
            instance = this;
        //else
        //  Destroy(this.gameObject);

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
    public void CalculateMatchResult()
    {
        int winScore = 0;
        int loseScore = 0;

        foreach (int score in listRoundResult)
        {
            if (score == WINROUND)
                winScore++;
            else if (score == LOSEROUND)
                loseScore++;
        }

        if (winScore == loseScore)
        {
            MatchResult(DRAWMATCH);
        }
        else if (winScore > loseScore)
        {
            MatchResult(WINMATCH);
        }
        else
        {
            MatchResult(LOSEMATCH);
        }
    }

    public void MatchResult(int matchResult, bool isQuit = false)
    {
        string userId;
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
            userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        else
            userId = manualUser;

        PlayerController playerController = PlayerController.GetInstance();

        if (gamePage == null)
            gamePage = GameObject.Find("GameUserInterface").GetComponent<GameUserIntefacePage>();

        if (isQuit)
        {
            playerController.UpdateHistory(-1, LOSEBATTLEPOINT, userId);
            return;
        }

        switch (matchResult)
        {
            case WINMATCH:
                playerController.UpdateHistory(1, WINBATTLEPOINT, userId);
                gamePage.WinMessage(WINBATTLEPOINT);
                break;
            case DRAWMATCH:
                playerController.UpdateHistory(0, DRAWBATTLEPOINT, userId);
                gamePage.DrawMessage(DRAWBATTLEPOINT);
                break;
            case LOSEMATCH:
                playerController.UpdateHistory(-1, LOSEBATTLEPOINT, userId);
                gamePage.LoseMessage(LOSEBATTLEPOINT);
                break;
        }
    }

    void SaveTempState()
    {
        PlayerController playerController = PlayerController.GetInstance();

        History history = playerController.GetHistory();

        history.BattlePoint = -8;

        string json = JsonUtility.ToJson(history);

        string SAVE_FILE = "tempState.dat";

        string filename = Path.Combine(Application.persistentDataPath + SAVE_FILE);

        File.WriteAllText(filename, json);
    }

    private void OnApplicationQuit()
    {
        Scene scene = SceneManager.GetActiveScene();

        // Check if the name of the current Active Scene is your first Scene.
        if (scene.name == "Match")
        {
            SaveTempState();
            return;
        }

        /*
        if (scene.name == "DraftPick")
        {
            view.RPC("CancelMatch", RpcTarget.Others);
        }*/

    }

    public void UpdateScore(int roundResult)
    {
        if (gamePage == null)
            gamePage = GameObject.Find("GameUserInterface").GetComponent<GameUserIntefacePage>();

        if (listRoundResult == null)
            listRoundResult = new List<int>();

        listRoundResult.Add(roundResult);

        gamePage.UpdateScore(roundResult);

    }

    public bool IsGameFinished()
    {
        if (listRoundResult == null)
            return false;
        if (listRoundResult.Count >= 5)
            return true;
        return false;
    }

    #endregion

    #region Load Scene
    public IEnumerator LoadScene(string sceneName, GameObject loadingUI, Text loadingText, Slider loadingSlider)
    {
        loadingUI.SetActive(true);

        PhotonNetwork.LoadLevel(sceneName);
        int titik = 1;
        string text = "Loading.";
        while (PhotonNetwork.LevelLoadingProgress < 1)
        {
            if (titik <= 3)
            {
                if (loadingText != null)
                    loadingText.text = text;
                text = text + ".";
                titik++;
            }
            if (titik == 3)
            {
                text = "Loading.";
            }

            if (loadingSlider != null)
                loadingSlider.value = PhotonNetwork.LevelLoadingProgress;

            yield return new WaitForEndOfFrame();
        }
    }
    #endregion

    [PunRPC]
    public void CancelMatch()
    {
        BackToMenu();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (cause.ToString() != "DisconnectByClientLogic")
        {
            Scene scene = SceneManager.GetActiveScene();

            // Check if the name of the current Active Scene is your first Scene.
            if (scene.name == "DraftPick")
                BackToMenu();
            if (scene.name == "Match")
            {
                RoundManager roundManager = GameObject.Find("RoundManager").GetComponent<RoundManager>();

                roundManager.ActiveReconnectSreen(true);

                PhotonNetwork.Reconnect();
            }

        }
    }

    public override void OnConnected()
    {
        
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master");

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("JOined to Lobby");

        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != "Match")
            return;

        RoundManager roundManager = GameObject.Find("RoundManager").GetComponent<RoundManager>().Instance;

        roundManager.ActiveReconnectSreen(false);

        MatchResult(LOSEMATCH);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Scene scene = SceneManager.GetActiveScene();

        if(scene.name == "DraftPick")
            BackToMenu();

        if(scene.name == "Match")
            MatchResult(1);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
        Destroy(this.gameObject);
    }
}
