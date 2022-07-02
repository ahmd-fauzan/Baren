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
using Google;
using Photon.Pun;

public class MainMenuManager : MonoBehaviour
{
    //Current Player ID
    //private string userID;

    //Current Player ID in Unity Editor
    //[SerializeField]
    //string manualUser;

    #region ScriptPage
    [SerializeField]
    private LeaderboardPage leaderbaordPage;

    [SerializeField]
    private CharacterPage characterPage;

    [SerializeField]
    private ProfilePage profilePage;
    #endregion

    [SerializeField]
    private MatchMakingManager matchManager;

    #region GameObject Content
    [SerializeField]
    private GameObject characterMenu;

    [SerializeField]
    private GameObject matchMenu;

    [SerializeField]
    private GameObject leaderboardMenu;

    [SerializeField]
    private GameObject profileMenu;

    [SerializeField]
    private GameObject helpMenu;

    [SerializeField]
    private GameObject pengenalanMenu;

    [SerializeField]
    private GameObject mapMenu;
    #endregion

    //Screen Loading
    [SerializeField]
    private GameObject loadingScreen;

    [SerializeField]
    private GameObject circleLoadingScreen;

    [SerializeField]
    private Image circleLoading;

    [SerializeField]
    AudioSource audioSource;

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

        pController = PlayerController.GetInstance();

        dbManager = ScriptableObject.CreateInstance<DatabaseManager>().GetInstance();

        dbManager.Reference.Child("Players").Child(pController.CurrentUserId).Child("PlayerInfo").ValueChanged -= HandleChangePlayerInfo;

        dbManager.Reference.Child("Players").Child(pController.CurrentUserId).Child("History").ChildAdded -= HandleHistoryChildAdded;
        dbManager.Reference.Child("Players").ChildAdded -= HandleLeaderboardChildAdded;

        dbManager.Reference.Child("Players").ChildChanged -= HandleLeaderboardChildChange;
    }

    private void Start()
    {
        loadingScreen.SetActive(true);

        TaskEvent += HandleTask;

        Debug.Log("Current User Id : " + pController.CurrentUserId);
        if(!pController.CheckGuestPlayer())
        {
            dbManager.Reference.Child("Players").Child(pController.CurrentUserId).Child("PlayerInfo").ValueChanged += HandleChangePlayerInfo;

            dbManager.Reference.Child("Players").Child(pController.CurrentUserId).Child("History").ChildAdded += HandleHistoryChildAdded;

            dbManager.GetLastIndex(pController.CurrentUserId).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Statistic : " + task.Result);
                    if (task.Result == 0)
                    {
                        Debug.Log("Statistic Jalan");
                        taskStatistic = true;
                        TaskEvent();
                    }
                }
            });
        }
        else
        {
            PlayerInfo pInfo = pController.LoadGuestPlayerInfo();
            List<History> historyList = pController.LoadGuestHistory();

            Debug.Log("Player Win Rate : " + pInfo.WinRate);

            if (pInfo != null)
                HandlePlayerInfo(pInfo);

            if(historyList != null)
            {
                HandleHistory(pController.GetGuestStatistic(historyList));
            }
            else {
                taskStatistic = true;
                TaskEvent();
            }
        }

        dbManager.Reference.Child("Players").ChildAdded += HandleLeaderboardChildAdded;

        dbManager.Reference.Child("Players").ChildChanged += HandleLeaderboardChildChange;

        characterPage.ShowAllCharacter();

        ChangeMenu("Match");

        GameObject.Find("MatchButton").GetComponent<Button>().Select();

        
    }

    //Close Loading Screen
    public void HandleTask()
    {
        if (taskLeaderboard && taskProfile && taskNetwork && taskStatistic)
        {
            audioSource.Play();

            loadingScreen.SetActive(false);
        }
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

            History history = new History();

            JsonUtility.FromJsonOverwrite(json, history);
            
            File.Delete(filename);

            pInfo.BattlePoint += history.BattlePoint;

            if (pController == null)
                pController = PlayerController.GetInstance();

            pController.UpdateUserData(pController.CurrentUserId, pInfo, history);
        }
        else
        {
            Debug.Log("File Not Found");
        }
    }

    //Change Content Menu
    public void ChangeMenu(string menuName)
    {
        characterPage.ShowAttribut(null);

        switch (menuName)
        {
            case "Character":
                characterMenu.SetActive(true);
                matchMenu.SetActive(false);
                leaderboardMenu.SetActive(false);
                helpMenu.SetActive(false);
                break;
            case "Match":
                characterMenu.SetActive(false);
                matchMenu.SetActive(true);
                leaderboardMenu.SetActive(false);
                helpMenu.SetActive(false);
                break;
            case "Leaderboard":
                characterMenu.SetActive(false);
                matchMenu.SetActive(false);
                leaderboardMenu.SetActive(true);
                helpMenu.SetActive(false);
                break;
            case "Profile":
                profileMenu.SetActive(!profileMenu.activeInHierarchy);
                helpMenu.SetActive(false);
                break;
            case "Help":
                helpMenu.SetActive(true);
                pengenalanMenu.SetActive(true);
                mapMenu.SetActive(false);
                break;
            case "Introduction":
                pengenalanMenu.SetActive(true);
                mapMenu.SetActive(false);
                break;
            case "Map":
                pengenalanMenu.SetActive(false);
                mapMenu.SetActive(true);
                break;
        }
    }
    
    public void BackButton(GameObject gameObject)
    {
        if (gameObject.activeInHierarchy)
            ChangeMenu("Match");
        else
            ChangeMenu("Introduction");
    }
    public void Logout()
    {
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("BarenAccount")))
        {
            dbManager.UpdateSignedIn(false, pController.CurrentUserId);
            PlayerPrefs.DeleteKey("BarenAccount");
        }
        else if(!pController.CheckGuestPlayer()){
            dbManager.UpdateSignedIn(false, pController.CurrentUserId);
            GoogleSignIn.DefaultInstance.SignOut();
        }

        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("Authentication");
    }

    #region HandleFirebaseChange
    public void HandlePlayerInfo(PlayerInfo info)
    {
        Scene scene = SceneManager.GetActiveScene();

        // Check if the name of the current Active Scene is your first Scene.
        if (scene.name != "Menu")
            return;

        profilePage.SetProfileInfo(info, pController.CurrentUserId);

        profilePage.SetPlayerInfo(info);

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

        if (leaderboardList.Find(p => p.Username == info.Username) != null){
            int index = leaderboardList.FindIndex(p => p.Username == info.Username);
            leaderboardList[index] = info;
        }else
            leaderboardList.Add(info);

        Debug.Log("Leaderboard : " + info.WinRate);

        leaderboardList = leaderboardList.OrderByDescending(p => p.BattlePoint).ThenByDescending(p => p.WinRate).ToList();

        Debug.Log("Order Leaderboard : " + leaderboardList[0].WinRate);

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

        HandleLeaderboard(pController.GetUser(args.Snapshot.Child("PlayerInfo")));
        Debug.Log("Sender : " + sender + " ARGS : " + args.Snapshot);
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
        Debug.Log("History Kosong : " + args);

        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        if(args.Snapshot != null)
            HandleHistory(pController.GetStatistic(args.Snapshot));
    }
    #endregion

    public void ShowObject(GameObject go)
    {
        go.SetActive(!go.activeInHierarchy);
    }

    public IEnumerator ActiveObject(GameObject go)
    {
        go.SetActive(true);
        float counter = 5f;

        while(counter > 0)
        {
            yield return new WaitForSeconds(1f);
            counter--;
        }
        go.SetActive(false);
    }
    
    public void ShowCircleLoading(bool isActive)
    {
        Debug.Log("Circle Loading");
        circleLoadingScreen.SetActive(isActive);
        if (isActive)
            StartCoroutine(StartLoading());

    }

    IEnumerator StartLoading()
    {
        float counter = 1f;
        while (counter >= 0)
        {
            yield return new WaitForSeconds(0.02f);
            circleLoading.fillAmount = counter;
            counter -= 0.02f;
        }

        StartCoroutine(StartLoading());
    }
}
