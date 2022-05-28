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
    private string userID;

    [SerializeField]
    private Transform spawn;

    [SerializeField]
    private GameObject attributBar;

    [SerializeField]
    private Slider runSpeedSlider;

    [SerializeField]
    private Slider walkSpeedSlider;

    [SerializeField]
    private Slider staminaSlider;

    [SerializeField]
    private Slider staminaRegenSlider;

    [SerializeField]
    private Slider accelerationSlider;

    [SerializeField]
    private GameObject characterMenu;

    [SerializeField]
    private GameObject matchMenu;

    [SerializeField]
    private GameObject leaderboardMenu;

    [SerializeField]
    private GameObject profileMenu;

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

    [SerializeField]
    private Text usernameText;

    [SerializeField]
    private Text battlePointText;

    [SerializeField]
    private GameObject leaderboardItem;

    [SerializeField]
    private Transform leaderboardSpawn;

    [SerializeField]
    private GameObject loadingScreen;

    private List<GameObject> leaderboardItemList;

    private const int MAXSTAMINA = 30;
    private const float MAXWALKSPEED = 0.7f;
    private const float MAXRUNSPEED = 3f;
    private const int MAXACCELERATION = 9;
    private const int MAXSTAMINAREGEN = 3;

    private Color stPlaceColor = new Color32(255, 215, 0, 255);
    private Color ndPlaceColor = new Color32(192, 192, 192, 255);
    private Color rdPlaceColor = new Color32(205, 127, 50, 255);
    private Color thPlaceColor = new Color32(130, 111, 102, 255);

    private Character currentSelected;

    public bool taskLeaderboard;
    public bool taskProfile;
    public bool taskNetwork;
    public bool taskStatistic;

    List<PlayerInfo> leaderboardList;

    MainMenuManager instance;

    [SerializeField]
    string manualUser;

    bool allowQuit;

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
    public event TaskDelegate taskEvent;

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

        taskEvent += HandleTask;

        pController = PlayerController.GetInstance();

        leaderboardItemList = new List<GameObject>();

        dbManager.Reference.Child("Players").Child(userID).Child("PlayerInfo").ValueChanged += HandleChangePlayerInfo;

        dbManager.Reference.Child("Players").Child(userID).Child("History").ChildAdded += HandleHistoryChildAdded;

        dbManager.Reference.Child("Players").ChildAdded += HandleLeaderboardChildAdded;

        dbManager.Reference.Child("Players").ChildChanged += HandleLeaderboardChildChange;

        ShowAllCharacter();

        allowQuit = false;
    }

    public void HandleTask()
    {
        if (taskLeaderboard && taskProfile && taskNetwork && taskStatistic)
            loadingScreen.SetActive(false);
    }

    private void ShowAllCharacter()
    {
        CharacterController controller = GameObject.Find("CharacterController").GetComponent<CharacterController>();
        RectTransform rect = spawn.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, 319 * 4);
        Character[] characterList = controller.GetCharacterList();

        for (int i = 0; i < characterList.Length; i++)
        {
            GameObject go = SpawnManager.SpawnCard(characterList[i], spawn, false);
            AddEvent(go, characterList[i]);
        }
    }

    

    public void AddEvent(GameObject go, Character character)
    {
        if (go.GetComponent<EventTrigger>() == null)
        {
            go.AddComponent<EventTrigger>();
        }

        EventTrigger trigger = go.GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();

        entry.eventID = EventTriggerType.PointerClick;

        entry.callback.AddListener((functionIWant) => { ShowAttribut(character); });

        trigger.triggers.Add(entry);
    }

    private void ShowAttribut(Character character)
    {
        if (currentSelected != null)
        {
            if (currentSelected.characterId == character.characterId)
            {
                GameObject myEventSystem = GameObject.Find("EventSystem");
                myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);

                attributBar.SetActive(false);
                currentSelected = null;

                return;
            }
        }

        currentSelected = character;

        attributBar.SetActive(true);

        runSpeedSlider.maxValue = MAXRUNSPEED;
        walkSpeedSlider.maxValue = MAXWALKSPEED;
        staminaSlider.maxValue = MAXSTAMINA;
        staminaRegenSlider.maxValue = MAXSTAMINAREGEN;
        accelerationSlider.maxValue = MAXACCELERATION;

        runSpeedSlider.value = character.runSpeed;
        walkSpeedSlider.value = character.walkSpeed;
        staminaSlider.value = character.stamina;
        staminaRegenSlider.value = character.staminaRegen;
        accelerationSlider.value = character.acceleration;
    }

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

    IEnumerator SaveQuit()
    {
        History history = ScriptableObject.CreateInstance<History>();
        history.MatchResult = -1;
        history.MatchType = 0;
        history.BattlePoint = -8;

        var dbTask = dbManager.AddHistory(history, userID);

        yield return new WaitUntil(predicate: () => dbTask.IsCompleted);

        allowQuit = true;
        Application.Quit();
    }

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

    public void HandlePlayerInfo(PlayerInfo info)
    {
        Scene scene = SceneManager.GetActiveScene();

        // Check if the name of the current Active Scene is your first Scene.
        if (scene.name != "Menu")
            return;

        profileNameText.text = info.Username;

        usernameText.text = info.Username;
        battlePointText.text = info.BattlePoint.ToString();

        MatchMakingManager matchManager = GameObject.Find("MatchManager").GetComponent<MatchMakingManager>().GetInstance();
        matchManager.InitializeNetwork(info.Username);

        taskProfile = true;
        taskEvent();
    }

    public void HandleHistory(Statistic statistic)
    {
        Scene scene = SceneManager.GetActiveScene();

        // Check if the name of the current Active Scene is your first Scene.
        if (scene.name != "Menu")
            return;

        profileIdText.text = "#" + userID;

        winStatistic.text = statistic.Win.ToString();
        loseStatistic.text = statistic.Lose.ToString();
        drawStatistic.text = statistic.Draw.ToString();

        taskStatistic = true;
        taskEvent();
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

        int placePos = 0;
        foreach(GameObject go in leaderboardItemList)
        {
            Destroy(go);
        }

        foreach (PlayerInfo pInfo in leaderboardList)
        {
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

        taskLeaderboard = true;
        taskEvent();

    }

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

        HandleHistory(pController.GetStatistic(args));
    }

}
