using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Firebase.Database;
using Firebase.Extensions;
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

    private const int MAXSTAMINA = 30;
    private const float MAXWALKSPEED = 0.7f;
    private const float MAXRUNSPEED = 3f;
    private const int MAXACCELERATION = 9;
    private const int MAXSTAMINAREGEN = 3;

    private Character currentSelected;

    public delegate void DataChangedDelegate();
    public event DataChangedDelegate DataChangedEvent;

    private void Start()
    {
        PlayerController pController = GameObject.Find("PlayerController").GetComponent<PlayerController>().Instance;

        Debug.Log("User id " + pController.UserID);

        pController.DbReference.Child("Players").Child(pController.UserID).Child("History").ChildChanged += HandleChildChanged;
        
        ShowCharacter();

        ShowPlayerInfo();
    }
    private void ShowCharacter()
    {
        CharacterController controller = GameObject.Find("CharacterController").GetComponent<CharacterController>();
        RectTransform rect = spawn.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, 319 * 4);
        Character[] characterList = controller.GetCharacterList();

        for (int i = 0; i < characterList.Length; i++)
        {
            GameObject go = SpawnCard(characterList[i], spawn);
            AddEvent(go, characterList[i]);
        }
    }

    private GameObject SpawnCard(Character character, Transform spawn)
    {
        GameObject go = Instantiate(character.characterImage, spawn.position, spawn.rotation);
        //go.GetComponent<Image>().sprite = sprites[i]; //Set the Sprite of the Image Component on the new GameObject
        RectTransform rect = go.GetComponent<RectTransform>();

        rect.SetParent(spawn.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel
        go.GetComponent<Transform>().localScale = Vector3.one;

        go.GetComponent<Transform>().GetChild(2).gameObject.SetActive(false);

        return go;
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
        }
    }

    public void ShowProfile()
    {
        PlayerController pController = GameObject.Find("PlayerController").GetComponent<PlayerController>().Instance;

        profileMenu.SetActive(!profileMenu.activeInHierarchy);

        DatabaseManager dbManager = ScriptableObject.CreateInstance<DatabaseManager>();

        dbManager.GetPlayerInfo(pController.DbReference, pController.UserID).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                PlayerInfo info = task.Result;
                Debug.Log("Profile Name : " + info.Username);
                profileNameText.text = info.Username;
            }
        });

        dbManager.GetStatistic(pController.DbReference, pController.UserID).ContinueWithOnMainThread(task2 =>
        {
            if (task2.IsCompleted)
            {
                Debug.Log("Completed");
                Statistic statistic = task2.Result;

                profileIdText.text = "#" + pController.UserID;

                winStatistic.text = statistic.Win.ToString();
                loseStatistic.text = statistic.Lose.ToString();
                drawStatistic.text = statistic.Draw.ToString();

            }

            if (task2.IsFaulted)
            {
                Debug.Log("Error : " + task2.Exception);
            }
            Debug.Log("PlayerInfo");
        });
    }

    void ShowPlayerInfo()
    {
        PlayerController pController = GameObject.Find("PlayerController").GetComponent<PlayerController>().Instance;

        DatabaseManager dbManager = ScriptableObject.CreateInstance<DatabaseManager>();

        Debug.Log("User id : " + pController.UserID);

        dbManager.GetPlayerInfo(pController.DbReference, pController.UserID).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                PlayerInfo info = task.Result;

                usernameText.text = info.Username;
                battlePointText.text = info.BattlePoint.ToString();
            }
            if (task.IsFaulted)
            {
                Debug.Log(task.Exception);
            }
        });
    }

    /*public void HandleHistoryChange()
    {
        winStatistic.text = pController.Statistic.Win.ToString();
        loseStatistic.text = pController.Statistic.Lose.ToString();
        drawStatistic.text = pController.Statistic.Draw.ToString();
    }*/

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
