using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class MatchMakingManager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "0.0.1";

    [SerializeField]
    Text status;

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.CurrentRoom != null)
                PhotonNetwork.LeaveRoom();
            return;
        }

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.NickName = "Baren " + Random.Range(0, 999);
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    private void LoadScene()
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
        return gameObject.AddComponent<MatchMakingManager>();
    }

    public static void CreateRoom(string roomName, int status)
    {
        PlayerController playerController = GameObject.Find("PlayerController").GetComponent<PlayerController>();

        History history = (ScriptableObject.CreateInstance<History> ());
        history.MatchType = status;

        playerController.AddHistoy(history);

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 2;

        if (status == 0)
        {
            options.IsVisible = false;
        }

        PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined to Lobby");
        status.text = "Joined to Lobby";

    }
    public static void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master");
        status.text = "Connected to master";
        PhotonNetwork.JoinLobby();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Connected to master");
        status.text = "Leaved Room";
    }
    public override void OnCreatedRoom()
    {
        Debug.Log("Room berhasil dibuat");
        status.text = "Room berhasil dibuat";
        Debug.Log("Room Name : " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined to Room");
        status.text = "Joined to Room";
        LoadScene();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        LoadScene();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Room tidak ditemukan, room akan dibuat");
        //CreateRoom();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
    }
}
