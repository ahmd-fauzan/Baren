using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using UnityEngine.UI;
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

    [SerializeField]
    private GameObject characterTimer;

    Vector3 cursorPosition;

    Vector3 truePos;
    public float gridSize;

    [SerializeField]
    private GameObject structure;

    private int countMyPrisoner = 0;
    private int countEnemyPrisoner = 0;

    private string otherPlayerName;

    private GameUserIntefacePage gamePage;

    private List<BaseLocation> bases;

    PhotonView view;

    List<Character> selectedCharacters;

    [SerializeField]
    int countCharacter;

    [SerializeField]
    private bool inRound;

    [SerializeField]
    private int playerWin;
    private int enemyWin;

    private float firstClickTime;
    private float timeBetweenClicks;
    private int clickCounter;
    private bool coroutineAllowed;

    private const int ENEMYLEFT = -1;
    private const int DRAWROUND = 0;
    private const int LOSEROUND = 1;
    private const int WINROUND = 2;

    private const int WALK = 1;
    private const int RUN = 2;

    private const int WINBATTLEPOINT = 10;
    private const int LOSEBATTLEPOINT = -8;

    private int costPoint;

    private Color myImageColor = new Color32(154, 154, 154, 140);
    private Color enemyImageColor = new Color32(48, 48, 48, 140);

    Coroutine draftCoroutine;
    Coroutine roundCoroutine;
    Coroutine characterCoroutine;

    List<GameObject> cardAvailable;

    [SerializeField]
    private bool resetingPosition;

    public delegate void ResetPositionCallback();
    ResetPositionCallback resetPositionCallback;

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

        costPoint = 100;

        DraftPick();
    }

    private void Start()
    {
        firstClickTime = 0f;
        timeBetweenClicks = 0.2f;
        clickCounter = 0;
        coroutineAllowed = true;
    }
    // Update is called once per frame
    void Update()
    {
        GameObject[] allGo = GameObject.FindGameObjectsWithTag("Character");

        if (enemyCharacter.Count <= countCharacter)
        {
            foreach (GameObject go in allGo)
            {
                if (!go.GetComponent<PhotonView>().IsMine)
                    enemyCharacter.Add(go);
            }
        }

        //Touch touch = Input.GetTouch(0);

        if (Input.GetMouseButtonUp(0))//touch.phase == TouchPhase.Began)
        {
            clickCounter += 1;

            Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);//Input.GetTouch(0).position);

            //float timeSinceLastClick = Time.time - lastTimeClick;

            TouchCharacter(ray);

            if (clickCounter == 1 && coroutineAllowed)
            {
                firstClickTime = Time.time;
                StartCoroutine(DoubleClickDetection(ray));
            }

            /*if (timeSinceLastClick < .2f)
            {
                Debug.Log("Time Click : " + timeSinceLastClick);
                TouchDestination(ray, 1);
            }
            else
                TouchDestination(ray, 2);*/


            //lastTimeClick = Time.time;
            //StartCoroutine("WaitClickTime");
        }

        

        if (characterSelected != null)
        {
            if (characterSelected.GetComponent<CharacterMovement>().Value == -1)
                characterSelected = null;
        }

        if(bases != null)
        {
            //if (IsCharacterInBase() && !IsGameFinished())
              //  inRound = true;

            UpdateCharacterRotation();
        }

        if (resetingPosition)
        {
            if (IsCharacterInBase())
            {
                resetingPosition = false;
                PlayRound();
            }
                
        }
            
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        playerWin = 3;
        CalculateRoundResult();
    }

    public void StartCharacterTimer()
    {
        characterTimer.SetActive(false);
        characterCoroutine = StartCoroutine(Countdown(3, 20));
    }

    IEnumerator DoubleClickDetection(Ray ray)
    {
        coroutineAllowed = false;
        while (Time.time < firstClickTime + timeBetweenClicks)
        {
            if(clickCounter == 2)
            {
                TouchDestination(ray, RUN);
                break;
            }
            yield return new WaitForEndOfFrame();
        }

        if (clickCounter == 1)
            TouchDestination(ray, WALK);

        clickCounter = 0;
        firstClickTime = 0;
        coroutineAllowed = true;
    }
    private bool IsCharacterInBase()
    {
        foreach (BaseLocation b in bases)
        {
            if (!b.Filled && b.Character.GetComponent<CharacterMovement>().Value != 0)
                return false;
        }
        return true;
    }

    private void CalculateRoundResult()
    {
        PlayerController playerController = GameObject.Find("PlayerController").GetComponent<PlayerController>();

        if (playerWin == 3)
        {
            playerController.UpdateHistory(1, WINBATTLEPOINT);
            gamePage.WinMessage(WINBATTLEPOINT);
        }

        else
        {
            playerController.UpdateHistory(1, -LOSEBATTLEPOINT);
            gamePage.LoseMessage(-LOSEBATTLEPOINT);
        }
    }

    IEnumerator Countdown(int timerType, int seconds)
    {
        int counter = seconds;
        while (counter > 0)
        {
            yield return new WaitForSeconds(1);
            counter--;
            if (timerType == 1)
                gamePage.SetDraftTimer(counter);
            else if (timerType == 2)
                gamePage.UpdateTimer(counter);
            else if(timerType == 3)
            {
                if (counter <= 5)
                    characterTimer.SetActive(true);
            }
        }
        if (timerType == 1)
        {
            gamePage.HideDraftPick();
            gamePage.ActiveRoundUI();
            PlayRound();
        }

        else if (timerType == 2)
            view.RPC("StorePrisoner", RpcTarget.Others, countMyPrisoner);
        else if (timerType == 3)
            RoundEnded(LOSEROUND);
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

    public string GetStatus()
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

    public void LoadScene() {
        PhotonNetwork.LoadLevel("Match");
    }

    public void UpdateBaseLocation(Transform character)
    {
        for (int i = 0; i < bases.Count; i++)
        {
            if (bases[i].Character.name == character.name)
            {
                if (bases[i].Filled)
                {
                    if (character.GetComponent<CharacterMovement>().Value == 0)
                        bases[i].Filled = false;
                }
                else
                {
                    if (character.GetComponent<CharacterMovement>().Value != 0)
                        bases[i].Filled = true;
                }
            }
        }
    }

    public void ReleaseCharacter()
    {
        foreach (BaseLocation b in bases)
        {
            CharacterMovement charMove = b.Character.GetComponent<CharacterMovement>();

            if (!b.Filled && charMove.Value == -1)
            {
                charMove.Status = 1;
                charMove.Move(b.Location.position, WALK);
            }
            countMyPrisoner = 0;
        }
    }

    private void DraftPick()
    {
        gamePage.SetOtherPlayerName(otherPlayerName);
        SpawnDraftCharacter();
        StartCoroutine(Countdown(1, 15));
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

            GameObject go = SpawnCard(list[i], spawn, false);
            AddEvent(new GameObject[] { go }, list[i]);
            
            if (cardAvailable == null)
                cardAvailable = new List<GameObject>();

            cardAvailable.Add(go);
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
        if (character != null)
        {
            entry.callback.AddListener((functionIWant) => { UpdateCardImage(go[0], myStatus); });
            entry.callback.AddListener((functionIWant) => { AddCharacter(character); });
        }
        else
            entry.callback.AddListener((functionIWant) => { SetcharacterSelected(go[1].GetComponent<Transform>()); });

        trigger.triggers.Add(entry);
    }

    public void RemoveEventTrigger(EventTrigger eventTrigger) {
        if (!isAvailableSelect())
            return;

        eventTrigger.triggers.RemoveRange(0, eventTrigger.triggers.Count);
    }
    public void UpdateCardImage(GameObject go, string playerType)
    {
        Debug.Log(go.name);

        if (!isAvailableSelect())
            return;

        go.transform.GetChild(1).gameObject.SetActive(true);
        Image image = go.transform.GetChild(1).GetComponent<Image>();

        if (myStatus == playerType)
            image.color = myImageColor;
        else
            image.color = enemyImageColor;

        RemoveEventTrigger(go.GetComponent<EventTrigger>());
    }

    public void AddCharacter(Character character)
    {
        

        if (!isAvailableSelect())
            return;

        SpawnCard(character, myPoint, false);

        CallRPCMethod("SelectCharacter", character.characterId, character.cost, myStatus);

        if (selectedCharacters == null)
            selectedCharacters = new List<Character>();

        if (bases == null)
            bases = new List<BaseLocation>();

        Transform spawn;

        if (myStatus == "Player1")
            spawn = spawnLocations[bases.Count];
        else
            spawn = spawnLocations[bases.Count + 5];

        GameObject go = SpawnCharacter(character, spawn);
        
        bases.Add(new BaseLocation(spawn, go.transform, true));

        UpdateCostPoint(character.cost);
    }

    private bool isAvailableSelect()
    {
        if (bases == null)
            return true;

        if(bases != null)
        {
            if (bases.Count < 5 && costPoint > 0)
                return true;
        }

        return false;
    }

    private void UpdateCostPoint(int characterCost)
    {
        costPoint -= characterCost;
        gamePage.UpdateCostPoint(costPoint);
    }

    #endregion

    #region Round
    private void PlayRound()
    {
        if (!IsGameFinished())
        {
            inRound = true;

            if (roundCoroutine != null)
                StopCoroutine(roundCoroutine);
            if (characterCoroutine != null)
                StopCoroutine(characterCoroutine);

            characterTimer.SetActive(false);
            roundCoroutine = StartCoroutine(Countdown(2, 180));
            characterCoroutine = StartCoroutine(Countdown(3, 20));
        }
        else
            CalculateRoundResult();
    }

    public void RoundEnded(int roundResult)
    {
        if (!inRound)
            return;

        inRound = false;

        switch (roundResult)
        {
            case LOSEROUND:
                enemyWin++;
                view.RPC("RoundResult", RpcTarget.Others, WINROUND);
                break;
            case WINROUND:
                playerWin++;
                view.RPC("RoundResult", RpcTarget.Others, LOSEROUND);
                break;
        }

        if(playerWin == 3 || enemyWin == 3)
        {
            CalculateRoundResult();
            return;
        }

        StopCoroutine(roundCoroutine);
        StopCoroutine(characterCoroutine);

        gamePage.UpdateScore(roundResult);

        ResetCharacterPosition();
    }

    private void UpdateCharacterRotation()
    {
        foreach(BaseLocation b in bases)
        {
            CharacterMovement c = b.Character.GetComponent<CharacterMovement>();
            if(c.Value == -1)
            {
                Vector3 rotation;

                if(myStatus == "Player1")
                    rotation = prisonerLocations[5].GetChild(0).rotation.eulerAngles;
                else
                    rotation = prisonerLocations[0].GetChild(0).rotation.eulerAngles;

                b.Character.rotation = Quaternion.Euler(0, rotation.y, 0);
            }
            
            if(c.Value == 0)
            {
                Vector3 rotation = b.Location.rotation.eulerAngles;

                b.Character.rotation = Quaternion.Euler(0, rotation.y, 0);
            }
        }
    }
    private void ResetCharacterPosition()
    {
        resetingPosition = true;

        foreach(BaseLocation b in bases)
        {
            CharacterMovement character = b.Character.GetComponent<CharacterMovement>();
            if(!b.Filled && character.Value != 0)
            {
                character.Move(b.Location.position, WALK);
            }
        }
    }

    private bool IsGameFinished()
    {
        if (playerWin == 3 || enemyWin == 3 || (playerWin + enemyWin == 5))
            return true;
        return false;
    }

    private void CalculateResult()
    {
        if (countMyPrisoner < countEnemyPrisoner)
            RoundEnded(WINROUND);
        else if (countMyPrisoner > countEnemyPrisoner)
            RoundEnded(LOSEROUND);
        else
            RoundEnded(DRAWROUND);
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

        GameObject cardGo = SpawnCard(character, characterSelection.GetComponent<Transform>(), true);

        AddEvent(new GameObject[] { cardGo, go }, null);

        Slider slider = cardGo.GetComponent<Transform>().GetChild(2).GetComponent<Slider>();
        slider.maxValue = character.stamina;
        characterMovement.SetCharacter(character, slider);

        return go;
    }

    public GameObject SpawnCard(Character character, Transform spawn, bool sliderActive)
    {
        GameObject go = Instantiate(character.characterImage, spawn.position, spawn.rotation);
        //go.GetComponent<Image>().sprite = sprites[i]; //Set the Sprite of the Image Component on the new GameObject
        RectTransform rect = go.GetComponent<RectTransform>();
       
        rect.SetParent(spawn.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel
        go.GetComponent<Transform>().localScale = Vector3.one;

        go.GetComponent<Transform>().GetChild(2).gameObject.SetActive(sliderActive);

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
    void TouchDestination(Ray ray, int touchCount)
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (hit.collider.CompareTag("Ground") && !hit.collider.CompareTag("Character") && characterSelected != null)
            {
                CharacterMovement characterMovement = characterSelected.GetComponent<CharacterMovement>().GetInstance();
                characterMovement.Move(hit.point, touchCount);
                DestroyMarkLocation();
                SpawnMarkLocation(hit.point);

                if (characterCoroutine != null)
                    StopCoroutine(characterCoroutine);
                
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
    public void SelectCharacter(string characterId, int cost, string playerType)
    {
        CharacterController controller = GameObject.Find("CharacterController").GetComponent<CharacterController>();
        SpawnCard(controller.GetCharacterById(characterId), enemyPoint, false);
        Transform parent = GameObject.Find("Cost" + cost).transform.GetChild(0).GetComponent<Transform>();
        for (int i = 0; i < parent.childCount; i++)
        {
            Debug.Log(parent.GetChild(i).name + " : " + "Img" + characterId + "(Clone)");
            if(parent.GetChild(i).name == "Img" + characterId + "(Clone)")
            {
                UpdateCardImage(parent.GetChild(i).gameObject, playerType);
                break;
            }
        }
        
        countCharacter++;
    }

    [PunRPC]
    public void RoundResult(int roundResult)
    {
        RoundEnded(roundResult);
    }

    [PunRPC]
    public void StorePrisoner(int countPrisoner)
    {
        countEnemyPrisoner = countPrisoner;
        CalculateResult();
    }
    #endregion

    public void CallRPCMethod(string methodName, string characterId, int cost, string playerType)
    {
        view.RPC(methodName, RpcTarget.Others, characterId, cost, playerType);
    }
}
