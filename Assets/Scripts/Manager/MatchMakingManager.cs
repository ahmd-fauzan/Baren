using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Firebase.Extensions;
using UnityEngine.EventSystems;
using Google;

public class MatchMakingManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject loadingScreen;

    private string gameVersion = "0.0.1";
    [SerializeField] Transform roomListContent;

    [SerializeField] GameObject roomListPrefab;

    [SerializeField] GameObject roomPage;

    [SerializeField] Text roomCode;

    [SerializeField] Text myNameText;

    [SerializeField] Text enemyNameText;

    public List<GameObject> roomList;

    private MatchMakingManager instance;

    [SerializeField]
    Button startButton;

    public delegate void LoadMainMenuDelegate();
    public event LoadMainMenuDelegate LoadMainMenuEvent;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Run");

        loadingScreen.SetActive(true);

        startButton.GetComponent<Button>().interactable = false;

        roomPage.SetActive(false);

        LoadMainMenuEvent += OnLoadMainMenu;

        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.CurrentRoom != null)
                PhotonNetwork.LeaveRoom();
            return;
        }

        PlayerController pController = GameObject.Find("PlayerController").GetComponent<PlayerController>().Instance;

        DatabaseManager dbManager = ScriptableObject.CreateInstance<DatabaseManager>();

        dbManager.GetPlayerInfo(pController.DbReference, pController.UserID).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                PlayerInfo pInfo = task.Result;

                PhotonNetwork.AutomaticallySyncScene = true;
                PhotonNetwork.NickName = pInfo.Username;
                PhotonNetwork.GameVersion = gameVersion;
                PhotonNetwork.ConnectUsingSettings();
                Debug.Log("Connecting");
            }
        });
    }

    private void OnLoadMainMenu()
    {
        loadingScreen.SetActive(false);
    }

    public void LoadScene()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            if(PhotonNetwork.IsMasterClient)
                StartCoroutine(GoScene("DraftPick"));
        }
    }

    public IEnumerator GoScene(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);

        while (PhotonNetwork.LevelLoadingProgress < 1)
        {
            Debug.Log("Loading Level : " + (PhotonNetwork.LevelLoadingProgress * 100) + "%");
            yield return new WaitForEndOfFrame();
        }
    }

    public MatchMakingManager GetInstance()
    {
        if (instance == null)
            instance = this;

        return instance;
    }

    public static void CreateRoom(string roomName, int status)
    {
        PlayerController playerController = GameObject.Find("PlayerController").GetComponent<PlayerController>();

        History history = (ScriptableObject.CreateInstance<History> ());
        history.MatchType = status;

        playerController.AddHistoy(history);

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 2;
        options.IsOpen = true;

        Debug.Log("Status : " + status);
        if (status == 0)
        {
            Debug.Log("Visible");
            options.IsVisible = false;
        }
        else
        {
            Debug.Log("Invisible");
            options.IsVisible = true;

        }

        PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined to Lobby");
        LoadMainMenuEvent();
    }
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
        Debug.Log("Joining Room" + roomName);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master");

        PhotonNetwork.JoinLobby();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left Room");
    }
    public override void OnCreatedRoom()
    {
        Debug.Log("Room berhasil dibuat");
        Debug.Log("Room Name : " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined to Room");
        roomPage.SetActive(true);
        roomCode.text = "Room Code" + "\n" + PhotonNetwork.CurrentRoom.Name;
        SetPlayerInLobby();
        UpdateStartButton();
        //LoadScene();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        SetPlayerInLobby();
        startButton.GetComponent<Button>().interactable = true;
        //Instantiate(playerListPrefab, playerListContent).GetComponent<PlayerListItem>().Setup(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Another player left room");
        startButton.GetComponent<Button>().interactable = false;
        enemyNameText.text = PhotonNetwork.CurrentLobby.ToString();

    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Room tidak ditemukan, room akan dibuat");
        //CreateRoom();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        LeaveLobby();
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
    }

    #region RooList
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Update Room List");
        Debug.Log(roomList.Count);

        foreach (GameObject trans in this.roomList)
        {
            Destroy(trans);
        }
        for (int i = 0; i < roomList.Count; i++)
        {
            Debug.Log("Player : " + roomList[i].PlayerCount);

            if (roomList[i].PlayerCount < 2 && roomList[i].PlayerCount > 0)
            {
                GameObject addRoomList = Instantiate(roomListPrefab, roomListContent);
                this.roomList.Add(addRoomList);
                //addRoomList.transform.SetParent(roomListContent);
                SetRoomInfo(addRoomList, roomList[i]);
            }
        }
    }

    private void SetRoomInfo(GameObject roomGo, RoomInfo info)
    {
        string[] str = info.Name.Split(char.Parse("#"));

        roomGo.transform.GetChild(0).GetComponent<Text>().text = str[0];
        roomGo.transform.GetChild(1).GetComponent<Text>().text = str[1];
        AddEvent(roomGo.transform.GetChild(2).gameObject, info.Name);
    }

    public void AddEvent(GameObject go, string roomName)
    {
        if (go.GetComponent<EventTrigger>() == null)
        {
            go.AddComponent<EventTrigger>();
        }

        EventTrigger trigger = go.GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();

        entry.eventID = EventTriggerType.PointerClick;

        entry.callback.AddListener((functionIWant) => { JoinRoom(roomName); });

        trigger.triggers.Add(entry);
    }
    #endregion

    #region Lobby
    private void SetPlayerInLobby()
    {
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            if(player == PhotonNetwork.LocalPlayer)
            {
                myNameText.text = player.NickName;
            }

            else
            {
                enemyNameText.text = player.NickName;
            }
        }
    }

    private void UpdateStartButton()
    {
        if (!PhotonNetwork.IsMasterClient)
            startButton.gameObject.SetActive(false);
        else
            startButton.gameObject.SetActive(true);
    }

    public void LeaveLobby()
    {
        roomPage.SetActive(false);
        PhotonNetwork.LeaveRoom();
    }

    #endregion

    public void OnSignOut()
    {
        GoogleSignIn.DefaultInstance.SignOut();
    }
}
