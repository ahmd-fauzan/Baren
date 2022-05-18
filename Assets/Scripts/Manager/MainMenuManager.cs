using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Firebase.Database;
using Firebase.Extensions;
using System.Linq;

public class MainMenuManager : MonoBehaviour
{
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

    private DatabaseManager dbManager;

    public delegate void TaskDelegate();
    public event TaskDelegate taskEvent;

    private void Start()
    {
        taskEvent += HandleTask;

        PlayerController pController = GameObject.Find("PlayerController").GetComponent<PlayerController>().Instance;

        dbManager = ScriptableObject.CreateInstance<DatabaseManager>().GetInstance();

        //pController.DbReference.Child("Players").Child(pController.UserID).Child("History").ChildChanged += HandleChildChanged;
        
        ShowCharacter();

        ShowProfile();

        ShowLeaderboard();
    }

    public void HandleTask()
    {
        if (taskLeaderboard && taskProfile && taskNetwork && taskStatistic)
            loadingScreen.SetActive(false);
    }

    private void ShowCharacter()
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

    public void ShowProfile()
    {
        PlayerController pController = GameObject.Find("PlayerController").GetComponent<PlayerController>().Instance;

        dbManager.GetPlayerInfo(pController.UserID).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                PlayerInfo info = task.Result;
                
                profileNameText.text = info.Username;

                usernameText.text = info.Username;
                battlePointText.text = info.BattlePoint.ToString();

                MatchMakingManager matchManager = GameObject.Find("MatchManager").GetComponent<MatchMakingManager>().GetInstance();
                matchManager.InitializeNetwork(info.Username);

                taskProfile = true;
                taskEvent();
            }
        });

        dbManager.GetStatistic(pController.UserID).ContinueWithOnMainThread(task2 =>
        {
            if (task2.IsCompleted)
            {
                Statistic statistic = task2.Result;

                profileIdText.text = "#" + pController.UserID;

                winStatistic.text = statistic.Win.ToString();
                loseStatistic.text = statistic.Lose.ToString();
                drawStatistic.text = statistic.Draw.ToString();

                taskStatistic = true;
                taskEvent();
            }

            if (task2.IsFaulted)
            {
                Debug.Log("Error : " + task2.Exception);
            }
            Debug.Log("PlayerInfo");
        });
    }

    public void ShowLeaderboard()
    {
        dbManager.GetLeaderboard().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                List<PlayerInfo> leaderboardList = task.Result.OrderByDescending((PlayerInfo p) => p.BattlePoint).ToList();

                Debug.Log("Leaderboard : " + task.Result.Count);
                int placePos = 0;

                foreach (PlayerInfo pInfo in leaderboardList)
                {
                    placePos++;

                    GameObject leaderboardGo = Instantiate(leaderboardItem, leaderboardSpawn);

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
                    
                    if(placePos == 10)
                    {
                        return;
                    }
                }

                taskLeaderboard = true;
                taskEvent();
            }
        });

    }

    void HandleChildAdded(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot
    }

    void HandleChildChanged(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        Debug.Log("Data Changed");
        Debug.Log(args.Snapshot);
    }

    void HandleChildRemoved(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot
    }

    void HandleChildMoved(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot
    }
}
