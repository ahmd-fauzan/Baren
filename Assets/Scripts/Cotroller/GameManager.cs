using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Transform[] spawnLocations;

    [SerializeField]
    private Transform[] prisonerLocations;

    [SerializeField]
    Transform cost30Point;

    [SerializeField]
    Transform cost20Point;

    [SerializeField]
    Transform cost10Point;

    [SerializeField]
    Transform myPoint;

    [SerializeField]
    Transform enemyPoint;

    [SerializeField]
    GameObject markLocation;

    [SerializeField]
    private string myStatus;

    private int currentValue;

    [SerializeField]
    Transform characterSelected;

    [SerializeField]
    private Camera currentCamera;

    [SerializeField]
    List<GameObject> enemyCharacter;

    [SerializeField]
    GameObject[][] mySpawnPoint;

    [SerializeField]
    GameObject characterSelection;

    [SerializeField]
    GameObject cardSelection;

    [SerializeField]
    private GameObject markObject;

    Vector3 cursorPosition;

    Vector3 truePos;
    public float gridSize;

    [SerializeField]
    private GameObject structure;

    private int countMyPrisoner = 0;

    private string otherPlayerName;

    private GameUserIntefacePage gamePage;

    private List<BaseLocation> bases;

    PhotonView view;

    List<Character> selectedCharacters;

    [SerializeField]
    int countCharacter;

    public class BaseLocation
    {
        public Transform Location { get; set; }
        public Transform Character { get; set; }
        public bool Filled { get; set; }
        public BaseLocation(Transform location, Transform character, bool filled)
        {
            this.Location = location;
            this.Character = character;
            this.Filled = filled;
        }

    }
    private void Awake()
    {
        view = GetComponent<PhotonView>();

        gamePage = GameObject.Find("GameUserInterface").GetComponent<GameUserIntefacePage>();

        myStatus = GetStatus();

        currentCamera = GameObject.Find("CameraManager").GetComponent<CameraManager>().GetCamera(myStatus);

        currentValue = 0;

        DraftPick();
    }


    // Update is called once per frame
    void Update()
    {
        
        GameObject[] allGo = GameObject.FindGameObjectsWithTag("Character");

        if(enemyCharacter.Count <= countCharacter)
        {
            foreach (GameObject go in allGo)
            {
                if (!go.GetComponent<PhotonView>().IsMine)
                    enemyCharacter.Add(go);
            }
        }
        
        //Touch touch = Input.GetTouch(0);

        if (Input.GetMouseButton(0))//touch.phase == TouchPhase.Began)
        {
            Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);//Input.GetTouch(0).position);
            
            TouchCharacter(ray);
            TouchDestination(ray);
        }

        if (characterSelected != null)
        {
            if (characterSelected.GetComponent<CharacterMovement>().Value == -1)
                characterSelected = null;
        }
    }

    IEnumerator Countdown(int seconds)
    {
        int counter = seconds;
        while (counter > 0)
        {
            yield return new WaitForSeconds(1);
            counter--;
            gamePage.SetDraftTimer(counter);
        }
        gamePage.HideDraftPick();
        //Instantiate();
    }

    /*private void LateUpdate()
    {
        Vector3 mousePos = Input.mousePosition;
        cursorPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));

        truePos.x = Mathf.Floor(cursorPosition.x / gridSize) * gridSize;
        truePos.y = structure.transform.position.y;
        truePos.z = Mathf.Floor(cursorPosition.z / gridSize) * gridSize;

        structure.transform.position = truePos;
        //structure.transform.position = currentCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 5f));

    }*/

    private string GetStatus()
    {
        if (PhotonNetwork.PlayerList[0] == PhotonNetwork.LocalPlayer)
        {
            otherPlayerName = PhotonNetwork.PlayerList[1].NickName;
            return "Player1";
        }
        else if (PhotonNetwork.PlayerList[1] == PhotonNetwork.LocalPlayer)
        {
            otherPlayerName = PhotonNetwork.PlayerList[0].NickName;
            return "Player2";
        }
        return "";
    }

    

    

    public int AddValue()
    {
        currentValue = currentValue + 1;
        return currentValue;
    }

    public void LoadScene(){
        PhotonNetwork.LoadLevel("Match");
    }

    public void UpdateBaseLocation(Transform character)
    {
        for(int i = 0; i < bases.Count; i++)
        {
            if(bases[i].Filled)
            {
                if (bases[i].Character.name == character.name)
                {
                    if (character.GetComponent<CharacterMovement>().Value == 0)
                        bases[i].Filled = false;
                    else
                        bases[i].Filled = true;
                }
            }
            
        }
    }

    public void ReleaseCharacter()
    {
        for(int i = bases.Count; i >= 0; i--)
        {
            CharacterMovement charMove = bases[i].Character.GetComponent<CharacterMovement>();

            if (charMove.Value == -1)
            {
                Vector3 location = GetSpawnLocation(bases[i].Character.GetComponent<Transform>());
                if (location != new Vector3(0, 0, 0))
                {
                    charMove.Status = 1;
                    charMove.Move(location);

                }
            }
            countMyPrisoner = 0;
        }
    }

    private void DraftPick()
    {
        gamePage.SetOtherPlayerName(otherPlayerName);
        SpawnDraftCharacter();
        StartCoroutine(Countdown(30));
    }

    public void SetcharacterSelected(Transform selected)
    {

        if (characterSelected == null)
        {
            characterSelected = StartOutline(selected);

        }
        else if (characterSelected.name != selected.name)
        {
            StopOutline(characterSelected);

            characterSelected = StartOutline(selected);
        }
    }

    #region Spawn Manager

    public void SpawnDraftCharacter()
    {
        Character[] list = GameObject.Find("CharacterController").GetComponent<CharacterController>().GetCharacterList();

        Transform spawn;
        for (int i = 0; i < list.Length; i++)
        {
            if (list[i].cost == 30)
            {
                spawn = cost30Point;
            }
            else if (list[i].cost == 20)
            {
                spawn = cost20Point;
            }
            else
            {
                spawn = cost10Point;
            }

            GameObject go = SpawnCard(list[i], spawn);
            AddEvent(new GameObject[] { go }, list[i]);
        }
    }

    public void AddEvent(GameObject[] go, Character character)
    {
        if (go[0].GetComponent<EventTrigger>() == null)
        {
            go[0].AddComponent<EventTrigger>();
        }

        EventTrigger trigger = go[0].GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        if(character != null)
            entry.callback.AddListener((functionIWant) => { AddCharacter(character); });
        else
            entry.callback.AddListener((functionIWant) => { SetcharacterSelected(go[1].GetComponent<Transform>()); });
        trigger.triggers.Add(entry);
    }

    public void AddCharacter(Character character)
    {
        SpawnCard(character, myPoint);

        CallRPCMethod("SelectCharacter", character.characterId);

        if (selectedCharacters == null)
            selectedCharacters = new List<Character>();

        if (bases == null)
            bases = new List<BaseLocation>();

        Transform spawn;
        if(myStatus == "Player1")
            spawn = spawnLocations[bases.Count];
        else
            spawn = spawnLocations[bases.Count + 5];

        GameObject go = SpawnCharacter(character, spawn);
        GameObject cardGo = SpawnCard(character, characterSelection.GetComponent<Transform>());

        AddEvent(new GameObject[] { cardGo, go }, null);
        bases.Add(new BaseLocation(spawn, go.transform, true));
    }

    #endregion

    public void DestroyMarkLocation()
    {
        if (markObject != null)
            Destroy(markObject);
    }

    #region Spawn Object
    public GameObject SpawnCharacter(Character character, Transform spawnPoint)
    {
        GameObject go = PhotonNetwork.Instantiate(Path.Combine("Prefab", character.characterId), spawnPoint.transform.position, spawnPoint.transform.rotation);

        CharacterMovement characterMovement = go.GetComponent<CharacterMovement>();

        characterMovement.SetAttribute(character.runSpeed, character.acceleration, character.stamina, character.characterName, 0);

        return go;
    }

    public GameObject SpawnCard(Character character, Transform spawn)
    {
        GameObject go = Instantiate(character.characterImage, spawn.position, spawn.rotation);
        //go.GetComponent<Image>().sprite = sprites[i]; //Set the Sprite of the Image Component on the new GameObject
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(spawn.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel
        return go;
    }

    public void SpawnMarkLocation(Vector3 location)
    {
        markObject = Instantiate(markLocation, location, Quaternion.identity);
    }

    #endregion

    #region Event

    #endregion

    #region GetLocation

    public Vector3 GetPrisoner()
    {
        Vector3 location;

        if (myStatus == "Player1")
        {
            location = prisonerLocations[5 + countMyPrisoner].GetChild(0).position;
        }
        else
        {
            location = prisonerLocations[countMyPrisoner].GetChild(0).position;
        }
        countMyPrisoner++;
        return location;
    }

    public Vector3 GetSpawnLocation(Transform character)
    {
        foreach (BaseLocation b in bases)
        {
            Debug.Log("Update Base : " + b.Character + " : " + character.name);

            if(b.Character.name == character.name)
            {
                if (!b.Filled)
                {
                    return b.Location.position;
                }
            }
        }

        return new Vector3(0, 0, 0);
    }

    #endregion

    #region Touch
    void TouchDestination(Ray ray)
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (hit.collider.CompareTag("Ground") && !hit.collider.CompareTag("Character") && characterSelected != null)
            {
                CharacterMovement characterMovement = characterSelected.GetComponent<CharacterMovement>().GetInstance();
                characterMovement.Move(hit.point);
                DestroyMarkLocation();
                SpawnMarkLocation(hit.point);
            }
        }

    }

    void TouchCharacter(Ray ray)
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Debug.DrawLine(transform.position, hit.point, Color.red);

            if (hit.collider.CompareTag("Character"))
            {
                SetcharacterSelected(hit.transform);
            }
        }
    }
    #endregion

    #region Ouline
    Transform StartOutline(Transform selected)
    {
        CharacterMovement characterMovement = selected.GetComponent<CharacterMovement>().GetInstance();
        GetGreenIdOutline(characterMovement.Value);
        if (characterMovement.StartOutline())
            return selected;
        else
            return null;
    }

    void StopOutline(Transform selected)
    {
        CharacterMovement characterMovement = selected.GetComponent<CharacterMovement>().GetInstance();
        characterMovement.StopOutline();
    }

    public void GetGreenIdOutline(int value)
    {

        foreach (GameObject go in enemyCharacter)
        {
            PhotonView enemyView = go.GetComponent<PhotonView>();
            if (!enemyView.IsMine)
            {
                CharacterMovement character = go.GetComponent<CharacterMovement>();
                if (character.Value < value && character.Value > 0)
                {
                    character.EnemyOutline(Color.green, 3);
                }
                else if (character.Value > value && character.Value > 0)
                    character.EnemyOutline(Color.red, 3);
            }
        }
    }

    #endregion

    #region RPC
    [PunRPC]
    public void SelectCharacter(string characterId)
    {
        CharacterController controller = GameObject.Find("CharacterController").GetComponent<CharacterController>();
        SpawnCard(controller.GetCharacterById(characterId), enemyPoint);
        countCharacter++;
    }
    #endregion

    public void CallRPCMethod(string methodName, string characterId)
    {
        view.RPC(methodName, RpcTarget.Others, characterId);
    }
}
