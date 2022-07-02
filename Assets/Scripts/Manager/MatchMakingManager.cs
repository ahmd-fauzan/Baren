using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Firebase.Extensions;
using UnityEngine.EventSystems;
using Google;
using System.Linq;
using UnityEngine.SceneManagement;

public class MatchMakingManager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "0.0.1";
    [SerializeField] Transform roomListContent;

    [SerializeField] GameObject roomListPrefab;

    [SerializeField] GameObject roomPage;

    [SerializeField] Text roomCode;

    [SerializeField] Text myNameText;

    [SerializeField] Text enemyNameText;

    public List<GameObject> roomGOList;

    [SerializeField] private MainMenuManager menuManager;

    [SerializeField] private MatchMakingPage page;

    [SerializeField]
    private string currentRoom;

    [SerializeField]
    Button startButton;

    // Start is called before the first frame update
    void Start()
    {
        startButton.GetComponent<Button>().interactable = false;

        roomPage.SetActive(false);
    }

    public void InitializeNetwork(string username)
    {
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.CurrentRoom != null)
            {
                PhotonNetwork.LeaveRoom();
                return;
            }

            if(PhotonNetwork.CurrentLobby != null)
            {
                MainMenuManager menuManager = GameObject.Find("MainMenuManager").GetComponent<MainMenuManager>().Instance;
                menuManager.TaskNetwork = true;
                menuManager.HandleTask();
            }

                
        }

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.NickName = username;
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Connecting");
    }

    public void LoadScene()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsVisible = false;
                PhotonNetwork.CurrentRoom.IsOpen = false;
                StartCoroutine(GoScene("DraftPick"));
            }
        }
    }
    
    public void ChangeScene(string sceneName)
    {
        StartCoroutine(GoScene(sceneName));
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

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (cause.ToString() == "MaxCcuReached")
        {
            page.ShowServerFullScreen();
            Debug.Log("Server Full : " + cause);
            return;
        }

        if (cause.ToString() != "DisconnectByClientLogic")
        {
            MatchMakingPage page = GameObject.Find("MenuCanvas").GetComponent<MatchMakingPage>();

            page.ActiveReconnectSreen(true);

            PhotonNetwork.ReconnectAndRejoin();
        }
    }

    public void CreateRoom(string roomName, int status)
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 2;
        options.IsOpen = true;
        options.EmptyRoomTtl = 0;
        options.PlayerTtl = 60000;

        Debug.Log("Status : " + status);
        if (status == 0)
        {
            options.IsVisible = false;
        }
        else
        {
            options.IsVisible = true;
        }

        PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);

        menuManager.ShowCircleLoading(true);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined to Lobby : " + PhotonNetwork.CurrentLobby);

        MainMenuManager menuManager = GameObject.Find("MainMenuManager").GetComponent<MainMenuManager>().Instance;
        menuManager.TaskNetwork = true;
        menuManager.HandleTask();

        MatchMakingPage page = GameObject.Find("MenuCanvas").GetComponent<MatchMakingPage>();
        page.ActiveReconnectSreen(false);
        menuManager.ShowCircleLoading(false);
    }
    public void JoinRoom(string roomName)
    {
        menuManager.ShowCircleLoading(true);
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
        page.ShowScreen("roomScreen");
    }
    public override void OnCreatedRoom()
    {
        Debug.Log("Room berhasil dibuat");
        Debug.Log("Room Name : " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnJoinedRoom()
    {
        PlayerController playerController = PlayerController.GetInstance();
        History history = new History();

        if (PhotonNetwork.CurrentRoom.IsVisible)
        {
            history.MatchType = 1;
            page.ShowPopUp("close");
        }
        else
        {
            history.MatchType = 0;
            page.ShowPopUp("roomCodeScreen");
        }

        playerController.AddHistoy(history);

        page.ShowScreen("lobbyScreen");

        ShowLobby();
        UpdateStartButton();

        currentRoom = PhotonNetwork.CurrentRoom.Name;
        Destroy(roomGOList.Find(x => x.name == currentRoom));
        roomGOList.Remove(roomGOList.Find(x => x.name == currentRoom));

        page.ActiveReconnectSreen(false);
        //LoadScene();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        ShowLobby();
        startButton.GetComponent<Button>().interactable = true;
        //Instantiate(playerListPrefab, playerListContent).GetComponent<PlayerListItem>().Setup(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Another player left room");
        startButton.GetComponent<Button>().interactable = false;
        enemyNameText.text = ".....";

    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        menuManager.ShowCircleLoading(false);
        MatchMakingPage page = GameObject.Find("MenuCanvas").GetComponent<MatchMakingPage>();

        switch (returnCode)
        {
            case ErrorCode.GameFull:
                page.ShowErrorMessage("Room sudah full");
                break;
            case ErrorCode.GameDoesNotExist:
                page.ShowErrorMessage("Room tidak ditemukan");
                break;
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        UpdateStartButton();
        //LeaveLobby();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        menuManager.ShowCircleLoading(false);
        MatchMakingPage page = GameObject.Find("MenuCanvas").GetComponent<MatchMakingPage>();

        page.ShowErrorMessage("Room Sudah Ada");
    }

    #region RooList
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(RoomInfo info in roomList)
        {
            if(roomGOList.Count == 1)
            {
                if(info.PlayerCount == 0)
                {
                    Destroy(roomGOList[0]);
                    roomGOList.RemoveAt(0);
                }
            }

            if (info.PlayerCount == 2)
            {
                Destroy(roomGOList.Find(x => x.name == info.Name));
                roomGOList.RemoveAt(roomGOList.FindIndex(x => x.name == info.Name));
            }
        }

        if(roomGOList.Count > roomList.Count)
        {
            var filtered = roomGOList
                   .Where(x => !roomList.Any(y => y.Name != x.name));

            Debug.Log("Filtered : " + filtered.ToList().Count);
            foreach (GameObject go in filtered.ToList())
            {
                Destroy(go);
                Debug.Log("Delete Go : " + go.name);
                roomGOList.Remove(go);
            }
        }
    

        foreach(RoomInfo info in roomList)
        {
            Debug.Log("Room : " + info);
            if (info.PlayerCount < 2 && info.PlayerCount > 0 && roomGOList.Find(x => x.name == info.Name) == null)
            {
                GameObject addRoomList = Instantiate(roomListPrefab, roomListContent);
                addRoomList.name = info.Name;
                this.roomGOList.Add(addRoomList);
                //addRoomList.transform.SetParent(roomListContent);
                SetRoomInfo(addRoomList, info);
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
    private void ShowLobby()
    {
        menuManager.ShowCircleLoading(false);

        myNameText.text = ".....";
        enemyNameText.text = ".....";
        roomPage.SetActive(true);
        if (!PhotonNetwork.CurrentRoom.IsVisible)
        {
            roomCode.gameObject.SetActive(true);
            roomCode.text = PhotonNetwork.CurrentRoom.Name;
        }
        else
            roomCode.gameObject.SetActive(false);

        foreach (Player player in PhotonNetwork.PlayerList)
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
        PhotonNetwork.LeaveRoom();
        menuManager.ShowCircleLoading(true);
    }

    #endregion
 }
