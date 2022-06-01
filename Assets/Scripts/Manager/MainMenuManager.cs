using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Firebase.Database;
using Firebase.Extensions;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenuManager : MonoBehaviour
{
    //Current Player ID
    private string userID;

    //Current Player ID in Unity Editor
    [SerializeField]
    string manualUser;

    #region ScriptPage
    [SerializeField]
    private LeaderboardPage leaderbaordPage;

    [SerializeField]
    private CharacterPage characterPage;

    [SerializeField]
    private ProfilePage profilePage;
    #endregion

    #region GameObject Content
    [SerializeField]
    private GameObject characterMenu;

    [SerializeField]
    private GameObject matchMenu;

    [SerializeField]
    private GameObject leaderboardMenu;

    [SerializeField]
    private GameObject profileMenu;
    #endregion

    //Screen Loading
    [SerializeField]
    private GameObject loadingScreen;

    #region Event
    public bool taskLeaderboard;
    public bool taskProfile;
    public bool taskNetwork;
    public bool taskStatistic;
    #endregion

    //Leaderboard Data
    List<PlayerInfo> leaderboardList;

    //Singleton
    MainMenuManager instance;

    public MainMenuManager Instance
    {
        get
        {
            if (instance == null)
                instance = this;
            return instance;
        }
    }

    public bool TaskNetwork
    {
        set { this.taskNetwork = value; }
    }

    private PlayerController pController;

    public delegate void TaskDelegate();
    public event TaskDelegate TaskEvent;

    private DatabaseManager dbManager;

    private void Awake()
    {
        Input.backButtonLeavesApp = true;
    }

    private void Start()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser == null)
            userID = manualUser;
        else
            userID = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        dbManager = ScriptableObject.CreateInstance<DatabaseManager>().GetInstance();

        TaskEvent += HandleTask;

        pController = PlayerController.GetInstance();

        dbManager.Reference.Child("Players").Child(userID).Child("PlayerInfo").ValueChanged += HandleChangePlayerInfo;

        dbManager.Reference.Child("Players").Child(userID).Child("History").ChildAdded += HandleHistoryChildAdded;

        dbManager.Reference.Child("Players").ChildAdded += HandleLeaderboardChildAdded;

        dbManager.Reference.Child("Players").ChildChanged += HandleLeaderboardChildChange;

        characterPage.ShowAllCharacter();

        ChangeMenu("Match");
    }

    //Close Loading Screen
    public void HandleTask()
    {
        if (taskLeaderboard && taskProfile && taskNetwork && taskStatistic)
            loadingScreen.SetActive(false);
    }

    //Load temp data
    void LoadTempState(PlayerInfo pInfo)
    {
        string SAVE_FILE = "tempState.dat";

        string filename = Path.Combine(Application.persistentDataPath + SAVE_FILE);

        if (File.Exists(filename))
        {
            Debug.Log("File Found");

            string json = File.ReadAllText(filename);

            History history = ScriptableObject.CreateInstance<History>();

            JsonUtility.FromJsonOverwrite(json, history);
            
            File.Delete(filename);

            dbManager.AddHistory(history, userID).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Store History Success");
                }
            });

            pInfo.BattlePoint += history.BattlePoint;

            dbManager.UpdatePlayerInfo(userID, pInfo).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Update Player Info Success");
                }
            });
        }
        else
        {
            Debug.Log("File Not Found");
        }
    }

    //Change Content Menu
    public void ChangeMenu(string menuName)
    {
        Debug.Log(menuName);
        switch (menuName)
        {
            case "Character":
                characterMenu.SetActive(true);
                matchMenu.SetActive(false);
                leaderboardMenu.SetActive(false);
                break;
            case "Match":
                characterMenu.SetActive(false);
                matchMenu.SetActive(true);
                leaderboardMenu.SetActive(false);
                break;
            case "Leaderboard":
                characterMenu.SetActive(false);
                matchMenu.SetActive(false);
                leaderboardMenu.SetActive(true);
                break;
            case "Profile":
                profileMenu.SetActive(!profileMenu.activeInHierarchy);
                break;
        }
    }

    #region HandleFirebaseChange
    public void HandlePlayerInfo(PlayerInfo info)
    {
        Scene scene = SceneManager.GetActiveScene();

        // Check if the name of the current Active Scene is your first Scene.
        if (scene.name != "Menu")
            return;

        profilePage.SetProfileInfo(info, userID);

        profilePage.SetPlayerInfo(info);

        MatchMakingManager matchManager = GameObject.Find("MatchManager").GetComponent<MatchMakingManager>().GetInstance();
        matchManager.InitializeNetwork(info.Username);

        taskProfile = true;
        TaskEvent();
    }

    public void HandleHistory(Statistic statistic)
    {
        Scene scene = SceneManager.GetActiveScene();

        // Check if the name of the current Active Scene is your first Scene.
        if (scene.name != "Menu")
            return;

        profilePage.SetStatisticInfo(statistic);

        taskStatistic = true;
        TaskEvent();
    }

    public void HandleLeaderboard(PlayerInfo info)
    {
        Scene scene = SceneManager.GetActiveScene();

        // Check if the name of the current Active Scene is your first Scene.
        if (scene.name != "Menu")
            return;

        if (leaderboardList == null)
            leaderboardList = new List<PlayerInfo>();

        leaderboardList.Add(info);

        leaderboardList = leaderboardList.OrderByDescending((PlayerInfo p) => p.BattlePoint).ToList();

        if(leaderboardList != null && leaderboardList.Count > 0)
            leaderbaordPage.UpdateLeaderboard(leaderboardList);

        taskLeaderboard = true;
        TaskEvent();
    }
    #endregion

    #region FirebaseEvent
    public void HandleChangePlayerInfo(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        PlayerInfo pInfo = pController.GetUser(args.Snapshot);
        pController.PlayerInfo = pInfo;
        HandlePlayerInfo(pInfo);

        LoadTempState(pInfo);
    }

    public void HandleLeaderboardChildChange(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        //Nilai leaderboard berubah
    }

    public void HandleLeaderboardChildAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        HandleLeaderboard(pController.GetUser(args.Snapshot.Child("PlayerInfo")));
    }

    public void HandleHistoryChildAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        HandleHistory(pController.GetStatistic(args.Snapshot));
    }
    #endregion
}
