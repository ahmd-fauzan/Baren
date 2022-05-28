using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraftPickManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    Text playerNameText;

    [SerializeField]
    Text draftTimer;

    [SerializeField]
    Transform characterAvailableContent;

    [SerializeField]
    Transform myPoint;

    [SerializeField]
    Transform enemyPoint;

    [SerializeField]
    Text costPointText;

    [SerializeField]
    GameObject loadingUI;

    [SerializeField]
    Slider loadingSlider;

    [SerializeField]
    Text loadingText;

    [SerializeField]
    GameObject reconnectScreen;

    List<GameObject> cardAvailable;

    List<Character> selectedCharacter;

    private string myStatus;

    private int costPoint;

    [SerializeField]
    int countEnemyCharacter;

    private Color myImageColor = new Color32(154, 154, 154, 140);
    private Color enemyImageColor = new Color32(48, 48, 48, 140);

    PhotonView view;

    GameManager gameManager;

    bool startTimer = false;
    double timerIncrementValue;
    double startTime;

    [SerializeField] double timer = 30;
    ExitGames.Client.Photon.Hashtable CustomeValue;

    public delegate void DraftFinishedDelegate();
    public event DraftFinishedDelegate draftFinishedEvent;

    private void Start()
    {
        draftFinishedEvent += OnDraftFinished;

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        view = GetComponent<PhotonView>();

        costPoint = 100;

        if (PhotonNetwork.PlayerList[0] == PhotonNetwork.LocalPlayer)
        {
            SetOtherPlayerName(PhotonNetwork.PlayerList[1].NickName);
            myStatus = "Player1";
        }
        else if (PhotonNetwork.PlayerList[1] == PhotonNetwork.LocalPlayer)
        {
            SetOtherPlayerName(PhotonNetwork.PlayerList[0].NickName);
            myStatus = "Player2";
        }


        if (selectedCharacter == null)
            selectedCharacter = new List<Character>();

        SpawnDraftCharacter();
        StartTimer();
    }

    void Update()
    {
        if (!startTimer) return;

        timerIncrementValue = PhotonNetwork.Time - startTime;

        SetDraftTimer((int)timer - (int)timerIncrementValue);

        if (timerIncrementValue >= timer)
        {
            startTimer = false;

            if (IsCancelMatch())
            {
                StartCoroutine(gameManager.LoadScene("Menu", loadingUI, loadingText, loadingSlider));
                Debug.Log("Back To Menu");
                return;
            }

            if(selectedCharacter.Count < 3)
                RandomCharacter();
            else
                draftFinishedEvent();
        }
    }

    private void OnDraftFinished()
    {
        StartCoroutine(gameManager.LoadScene("Match", loadingUI, loadingText, loadingSlider));
    }

    public void SetOtherPlayerName(string playerName)
    {
        playerNameText.text = playerName;
    }

    public void SetDraftTimer(int timer)
    {
        draftTimer.text = timer.ToString();
    }

    private bool IsCancelMatch()
    {
        if (selectedCharacter.Count < 3 && countEnemyCharacter < 3)
            return true;

        return false;
    }

    public void FilterCard(int cost)
    {
        Character[] list = GameObject.Find("CharacterController").GetComponent<CharacterController>().GetCharacterList();

        for (int i = 0; i < list.Length; i++)
        {
            if(list[i].cost != cost)
            {
                characterAvailableContent.GetChild(i).gameObject.SetActive(false);
            }

            if (list[i].cost == cost || cost == 0)
            {
                characterAvailableContent.GetChild(i).gameObject.SetActive(true);
            }
        }
    }

    public void SpawnDraftCharacter()
    {
        Character[] list = GameObject.Find("CharacterController").GetComponent<CharacterController>().GetCharacterList();

        cardAvailable = new List<GameObject>();

        for (int i = 0; i < list.Length; i++)
        {

            GameObject go = SpawnManager.SpawnCard(list[i], characterAvailableContent, false);
            AddEvent(go, list[i]);

            cardAvailable.Add(go);
        }
    }

    public void UpdateCardImage(GameObject go, Character character, string playerType)
    {
        Debug.Log(go.name);

        if (character != null)
        {
            if (!isAvailableSelect(character))
               return;
        }

        go.transform.GetChild(1).gameObject.SetActive(true);
        Image image = go.transform.GetChild(1).GetComponent<Image>();

        if (myStatus == playerType)
            image.color = myImageColor;
        else
            image.color = enemyImageColor;

        RemoveEventTrigger(go.GetComponent<EventTrigger>());
    }

    
    private bool isAvailableSelect(Character character)
    {
        if (selectedCharacter == null || character == null)
            return true;

    
        if (selectedCharacter != null)
        {
            if (selectedCharacter.Count < 5 && costPoint >= character.cost)
            {
                if (character != null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void StartTimer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            CustomeValue = new ExitGames.Client.Photon.Hashtable();
            startTime = PhotonNetwork.Time;
            startTimer = true;
            CustomeValue.Add("DraftTime", startTime);
            PhotonNetwork.CurrentRoom.SetCustomProperties(CustomeValue);
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (PhotonNetwork.IsMasterClient)
            return;

        startTime = double.Parse(PhotonNetwork.CurrentRoom.CustomProperties["DraftTime"].ToString());
        startTimer = true;
    }

    private void UpdateCostPoint(int characterCost)
    {
        costPoint -= characterCost;
        costPointText.text = costPoint + " Point";
    }

    public void AddCharacter(Character character)
    {
        if (!isAvailableSelect(character))
            return;
        
        SpawnManager.SpawnCard(character, myPoint, false);

        view.RPC("SelectCharacter", RpcTarget.Others, character.characterId, myStatus);

        selectedCharacter.Add(character);

        gameManager.CharacterSelected = selectedCharacter;
        gameManager.CountEnemyCharacter = countEnemyCharacter;

        UpdateCostPoint(character.cost);
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
        if (character != null)
        {
            entry.callback.AddListener((functionIWant) => { UpdateCardImage(go, character, myStatus); });
            entry.callback.AddListener((functionIWant) => { AddCharacter(character); });
        }

        trigger.triggers.Add(entry);
    }


    public void RemoveEventTrigger(EventTrigger eventTrigger)
    {
        //if (!isAvailableSelect(null))
            //return;

        eventTrigger.triggers.RemoveRange(0, eventTrigger.triggers.Count);
    }

    private void RandomCharacter()
    {
        CharacterController controller = GameObject.Find("CharacterController").GetComponent<CharacterController>();
        Character[] characters = controller.GetCharacterList();
        List<int> numbers = new List<int>();

        while (selectedCharacter.Count < 3)
        {
            int index = NewNumber(numbers, characters.Length);

            if (!IsCharacterSelected(characters[index]))
            {
                if (costPoint > characters[index].cost)
                {
                    AddCharacter(characters[index]);
                }

            }
        }

        draftFinishedEvent();
    }

    private int NewNumber(List<int> numbers, int r)
    {

        int a = 0;

        while (a == 0)
        {
            a = Random.Range(0, r);
            if (!numbers.Contains(a))
            {
                return a;
            }
            else
            {
                a = 0;
            }
        }
        return a;
    }

    private bool IsCharacterSelected(Character character)
    {
        foreach (Character c in selectedCharacter)
        {
            if (character.characterId == c.characterId)
                return true;
        }

        return false;
    }

    #region RPC
    [PunRPC]
    public void SelectCharacter(string characterId, string playerType)
    {
        CharacterController controller = GameObject.Find("CharacterController").GetComponent<CharacterController>();
        SpawnManager.SpawnCard(controller.GetCharacterById(characterId), enemyPoint, false);
        Transform parent = GameObject.Find("CharacterAvailableContent").GetComponent<Transform>();

        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).name == "Img" + characterId + "(Clone)")
            {
                UpdateCardImage(parent.GetChild(i).gameObject, null, playerType);
                //selectedCharacter.Add(controller.GetCharacterById(characterId));
                break;
            }
        }

        countEnemyCharacter++;
    }

    #endregion

    public void ActiveReconnectSreen(bool isActive)
    {
        reconnectScreen.SetActive(isActive);
    }
}
