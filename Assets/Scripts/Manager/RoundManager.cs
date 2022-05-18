using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using UnityEngine.SceneManagement;

public class RoundManager : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    [SerializeField]
    private Camera currentCamera;

    [SerializeField]
    private Transform spawnPoint;

    [SerializeField]
    private Transform spawnPoint2;

    [SerializeField]
    private Transform prisonerLocation;
    
    [SerializeField]
    private Transform prisonerLocation2;

    [SerializeField]
    GameObject markLocation;

    private int currentValue;

    [SerializeField]
    Transform characterSelected;

    [SerializeField]
    GameObject[][] mySpawnPoint;

    [SerializeField]
    GameObject characterSelection;

    [SerializeField]
    private GameObject markObject;

    [SerializeField]
    private GameObject characterTimer;

    [SerializeField]
    Transform myCharacterParent;

    [SerializeField]
    Transform enemyCharacterParent;

    [SerializeField]
    GameObject loadingUI;

    [SerializeField]
    GameObject roundResultUI;

    [SerializeField]
    Text roundResultText;

    [SerializeField]
    GameObject optionUi;

    [SerializeField]
    GameObject requestDrawUI;

    [SerializeField]
    Slider requestTimeSlider;

    [SerializeField]
    GameObject requestSurrendUi;

    /*//GRID SYSTEM VARIABLE
    Vector3 cursorPosition;

    Vector3 truePos;
    public float gridSize;

    [SerializeField]
    private GameObject structure;*/

    [SerializeField]
    private int countMyPrisoner = 0;
    private int countEnemyPrisoner = 0;

    private GameUserIntefacePage gamePage;

    private List<BaseLocation> bases;

    public PhotonView view;

    private float firstClickTime;
    private float timeBetweenClicks;
    private int clickCounter;
    private bool coroutineAllowed;

    private const int DRAWMATCH = 0;
    private const int ENEMYLEFT = -1;

    private const int DRAWROUND = 0;
    private const int LOSEROUND = 1;
    private const int WINROUND = 2;

    private const int WALK = 1;
    private const int RUN = 2;

    private GameManager gameManager;

    [SerializeField]
    private List<GameObject> enemyCharacter;

    private bool ready;

    bool startTimer = false;
    double timerIncrementValue;
    double startTime;
    [SerializeField] double timer = 180;
    ExitGames.Client.Photon.Hashtable CustomeValue;

    public delegate void CharacterInstantiatedDelegate();
    public event CharacterInstantiatedDelegate CharacterInstantiatedEvent;
    
    public class BaseLocation
    {
        public Vector3 Location { get; set; }
        public Transform Character { get; set; }

        public GameObject Card { get; set; }
        public bool Filled { get; set; }
        public BaseLocation(Vector3 location, Transform character, GameObject card, bool filled)
        {
            this.Location = location;
            this.Character = character;
            this.Card = card;
            this.Filled = filled;
        }

    }
    private void Awake()
    {
        view = GetComponent<PhotonView>();

        gamePage = GameObject.Find("GameUserInterface").GetComponent<GameUserIntefacePage>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        currentCamera = GameObject.Find("CameraManager").GetComponent<CameraManager>().GetCamera(gameManager.GetStatus());

        currentValue = 0;
    }

    private void Start()
    {
        CharacterInstantiatedEvent += OnCharacterInstantiated;

        enemyCharacter = new List<GameObject>();

        ready = false;

        firstClickTime = 0f;
        timeBetweenClicks = 0.2f;
        clickCounter = 0;
        coroutineAllowed = true;

        Instantiate();
    }

    // Update is called once per frame
    void Update()
    {
        GameObject[] allGo = GameObject.FindGameObjectsWithTag("Character");

        if (enemyCharacter.Count <= gameManager.CountEnemyCharacter)
        {
            foreach (GameObject go in allGo)
            {
                if (go == null)
                    return;

                if (!go.GetComponent<PhotonView>().IsMine)
                {
                    enemyCharacter.Add(go);
                    CharacterMovement charMove = go.GetComponent<CharacterMovement>();
                    CharacterController controller = GameObject.Find("CharacterController").GetComponent<CharacterController>();

                    char[] charArr = go.name.ToCharArray();
                    int i;
                    for(i = 0; i < charArr.Length; i++)
                    {
                        if(charArr[i] == '(')
                        {
                            break;
                        }
                    }

                    charMove.SetCharacter(controller.GetCharacterById(go.name.Substring(0, i)), null);

                }
            }
        }

        //Touch touch = Input.GetTouch(0);

        if (Input.GetMouseButtonUp(0))//touch.phase == TouchPhase.Began)
        {
            clickCounter += 1;

            Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);//Input.GetTouch(0).position);

            TouchCharacter(ray);

            if (clickCounter == 1 && coroutineAllowed)
            {
                firstClickTime = Time.time;
                StartCoroutine(DoubleClickDetection(ray));
            }
        }


        //Tidak bisa select karakter tawanan
        if (characterSelected != null)
        {
            if (characterSelected.GetComponent<CharacterMovement>().Value == -1)
                characterSelected = null;
        }

        if (!startTimer) return;

        timerIncrementValue = PhotonNetwork.Time - startTime;

        gamePage.UpdateTimer((int)timer - (int)timerIncrementValue);

        if (timerIncrementValue >= timer)
        {
            //Timer Completed
            startTimer = false;

            view.RPC("StorePrisoner", RpcTarget.Others, countMyPrisoner);
        }
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

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (PhotonNetwork.IsMasterClient)
            return;

        if (PhotonNetwork.CurrentRoom.CustomProperties["StartTime"] == null)
            return;

        startTime = double.Parse(PhotonNetwork.CurrentRoom.CustomProperties["StartTime"].ToString());
        startTimer = true;
    }

    public void OnCharacterInstantiated()
    {
        if (ready)
        {
            loadingUI.SetActive(false);
            ready = false;
            PlayRound();
        }
    }

    private void Instantiate()
    {
        if (gameManager != null)
        {
            int spawnIndex = 0;

            foreach (Character character in gameManager.CharacterSelected)
            {
                if (gameManager.GetStatus() == "Player1")
                    SpawnCharacter(character, GetSpawnLocation(spawnPoint, gameManager.CharacterSelected.Count, spawnIndex), spawnPoint.rotation);
                else
                    SpawnCharacter(character, GetSpawnLocation(spawnPoint2, gameManager.CharacterSelected.Count, spawnIndex), spawnPoint2.rotation);
                spawnIndex++;
            }

            view.RPC("SetReady", RpcTarget.Others);

            CharacterInstantiatedEvent();
        }
    }

    private Vector3 GetSpawnLocation(Transform centerPoint, int listCount, int index)
    {
        Debug.Log(centerPoint.position);
        float dist = 1f;

        float length = dist * (float)(listCount - 1);

        return new Vector3(centerPoint.position.x - ((length / 2) - (dist * index)), centerPoint.position.y, centerPoint.position.z);

    }

    IEnumerator DoubleClickDetection(Ray ray)
    {
        coroutineAllowed = false;
        while (Time.time < firstClickTime + timeBetweenClicks)
        {
            if (clickCounter == 2)
            {
                TouchDestination(ray, RUN);
                break;
            }
            yield return new WaitForEndOfFrame();
        }

        if (clickCounter == 1)
        {
            TouchDestination(ray, WALK);
        }
            

        clickCounter = 0;
        firstClickTime = 0;
        coroutineAllowed = true;
    }

    public void SetRound(int roundResult)
    {
        switch (roundResult)
        {
            case WINROUND:
                view.RPC("RoundResult", RpcTarget.Others, LOSEROUND);
                break;
            case LOSEROUND:
                view.RPC("RoundResult", RpcTarget.Others, WINROUND);
                break;
        }

        RoundEnded(roundResult);

    }

    

    private void DestroyMarkMarker()
    {
        if(markObject != null)
            Destroy(markObject, .5f);
    }
    public int AddValue()
    {
        currentValue = currentValue + 1;
        return currentValue;
    }

    public void UpdateBaseLocation(Transform character)
    {
        if (character == null)
            return;

        for (int i = 0; i < bases.Count; i++)
        {
            if (bases[i].Character.name == character.name)
            {
                if (bases[i].Filled)
                {
                    if (character.GetComponent<CharacterMovement>().Value != 0)
                    {
                        bases[i].Filled = false;
                        characterSelected = StartOutline(characterSelected, true);
                    }
                        
                }
                else
                {
                    if (character.GetComponent<CharacterMovement>().Value == 0)
                    {
                        bases[i].Filled = true;
                        characterSelected = StartOutline(characterSelected, false);
                    }
                        
                }
            }
        }
    }

    public void StartOneOutline(CharacterMovement charMov)
    {
        if (characterSelected == null)
            return;

        if (IsInBase(characterSelected))
            return;

        CharacterMovement charMovement = characterSelected.GetComponent<CharacterMovement>().GetInstance();

        if (charMovement.Value > charMov.Value)
            charMov.EnemyOutline(Color.green, 3);
        else if (charMovement.Value < charMov.Value)
            charMov.EnemyOutline(Color.red, 3);

    }

    public void ReleaseCharacter()
    {
        foreach (BaseLocation b in bases)
        {
            CharacterMovement charMove = b.Character.GetComponent<CharacterMovement>();
            if (charMove == null)
                return;

            if (!b.Filled && charMove.Value == -1)
            {
                charMove.Status = 1;
                view.RPC("MoveCharacterRPC", RpcTarget.All, charMove.GetCharacter().characterId, gameManager.GetStatus(), WALK, b.Location);
            }
            countMyPrisoner = 0;
        }
    }

    public void SetcharacterSelected(Transform selected)
    {

        if (characterSelected == null)
        {
            if(IsInBase(selected))
                characterSelected = StartOutline(selected, false);
            else
                characterSelected = StartOutline(selected, true);
        }

        else if (characterSelected.name != selected.name)
        {
            StopOutline(characterSelected);

            if (IsInBase(selected))
                characterSelected = StartOutline(selected, false);
            else
                characterSelected = StartOutline(selected, true);
        }
    }

    #region Spawn Manager

    public void AddEvent(GameObject go, Transform characterSelected)
    {
        if (go.GetComponent<EventTrigger>() == null)
        {
            go.AddComponent<EventTrigger>();
        }

        EventTrigger trigger = go.GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();

        entry.eventID = EventTriggerType.PointerClick;

        entry.callback.AddListener((functionIWant) => { SetcharacterSelected(characterSelected); });

        trigger.triggers.Add(entry);
    }

    #endregion

    #region Round
    private void PlayRound()
    {
        roundResultUI.SetActive(false);

        StartTimer();
    }

    /*
    bool IsAllInBase()
    {
        if (bases == null)
            return false;

        foreach(BaseLocation b in bases)
        {
            if (b.Filled == false)
                return false;
        }

        return true;
    }
    */

    /*
    private void ResetCharacter()
    {
        foreach(BaseLocation b in bases)
        {
            if (!b.Filled)
            {
                b.Character.gameObject.SetActive(false);
                b.Character.position = b.Location;
                b.Character.GetComponent<CharacterMovement>().Value = 0;
                b.Character.gameObject.SetActive(true);
            }
            
        }
        
        characterSelected = null;

        StartTimer();
    }*/

    private void DestroyCharacter()
    {
        foreach(BaseLocation b in bases)
        {
            PhotonNetwork.Destroy(b.Character.gameObject);
            Destroy(b.Card);
        }

        bases = new List<BaseLocation>();
        characterSelected = null;
        enemyCharacter = new List<GameObject>();

        Instantiate();
    }

    private void StartTimer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            CustomeValue = new ExitGames.Client.Photon.Hashtable();
            startTime = PhotonNetwork.Time;
            startTimer = true;
            CustomeValue.Add("StartTime", startTime);
            PhotonNetwork.CurrentRoom.SetCustomProperties(CustomeValue);
        }
    }

    public void RoundEnded(int roundResult)
    {
        roundResultUI.SetActive(true);
        startTimer = false;

        if (roundResult == WINROUND)
            roundResultText.text = "Win Round";
        else if (roundResult == LOSEROUND)
            roundResultText.text = "Lose Round";
        else
            roundResultText.text = "Draw Round";

        gameManager.UpdateScore(roundResult);

        if (!gameManager.IsGameFinished())
            DestroyCharacter();
        else
            gameManager.CalculateMatchResult();
    }

    /*
    private void UpdateCharacterRotation()
    {
        
        foreach (BaseLocation b in bases)
        {
            CharacterMovement c = b.Character.GetComponent<CharacterMovement>();
            if (c.Value == -1)
            {
                Vector3 rotation;

                if (gameManager.GetStatus() == "Player1")
                    rotation = prisonerLocations[5].GetChild(0).rotation.eulerAngles;
                else
                    rotation = prisonerLocations[0].GetChild(0).rotation.eulerAngles;

                b.Character.rotation = Quaternion.Euler(0, rotation.y, 0);
            }

            if (c.Value == 0)
            {
                Vector3 rotation;

                if (gameManager.GetStatus() == "Player1")
                    rotation = spawnPoint.rotation.eulerAngles;
                else
                    rotation = spawnPoint2.rotation.eulerAngles;

                b.Character.rotation = Quaternion.Euler(0, rotation.y, 0);
            }
        }
    }*/

  

    private void CalculateResult()
    {
        if (countMyPrisoner < countEnemyPrisoner)
            RoundEnded(WINROUND);
        else if (countMyPrisoner > countEnemyPrisoner)
            RoundEnded(LOSEROUND);
        else if (countMyPrisoner == countEnemyPrisoner)
            RoundEnded(DRAWROUND);
    }

    #endregion

    #region Spawn Object
    public GameObject SpawnCharacter(Character character, Vector3 spawnPoint, Quaternion rotation)
    {
        GameObject go = SpawnManager.PhotonSpawn(character, spawnPoint, rotation, myCharacterParent);

        CharacterMovement characterMovement = go.GetComponent<CharacterMovement>();

        GameObject cardGo = SpawnManager.SpawnCard(character, characterSelection.GetComponent<Transform>(), true);

        

        Slider slider = cardGo.GetComponent<Transform>().GetChild(2).GetComponent<Slider>();

        slider.maxValue = character.stamina;

        characterMovement.SetCharacter(character, slider);

        if (bases == null)
            bases = new List<BaseLocation>();

        bases.Add(new BaseLocation(spawnPoint, go.transform, cardGo, true));


        return go;
    }

    #endregion

    #region Event

    #endregion

    #region GetLocation

    public Vector3 GetPrisoner()
    {
        float dist = 0.75f * countMyPrisoner;

        countMyPrisoner++;

        if(gameManager.GetStatus() == "Player1")
            return new Vector3(prisonerLocation2.position.x, prisonerLocation2.position.y, prisonerLocation2.position.z - dist);
        else
            return new Vector3(prisonerLocation.position.x, prisonerLocation.position.y, prisonerLocation.position.z + dist);
    }

    public Vector3 GetSpawnLocation(Transform character)
    {
        foreach (BaseLocation b in bases)
        {
            if (b.Character.name == character.name)
            {
                if (!b.Filled)
                {
                    return b.Location;
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
                if (characterSelected == null)
                    return;

                DestroyMarkMarker();

                CharacterMovement characterMovement = characterSelected.GetComponent<CharacterMovement>().GetInstance();
                //characterMovement.Move(SpawnMarkLocation(hit.point), touchCount);

                view.RPC("MoveCharacterRPC", RpcTarget.All, characterMovement.GetCharacter().characterId, gameManager.GetStatus(), touchCount, hit.point);
                //StartCharacterTimer();
            }
        }

    }

    void TouchCharacter(Ray ray)
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Debug.DrawLine(transform.position, hit.point, Color.red);

            if(hit.collider != null)
            {
                if (hit.collider.CompareTag("Character"))
                {
                    SetcharacterSelected(hit.transform);
                }
            }
            
        }
    }
    #endregion

    private bool IsInBase(Transform character)
    {
        if (bases == null)
            return false;

        foreach(BaseLocation b in bases)
        {
            if(b.Character.name == character.name)
            {
                if (b.Filled)
                    return true;
            }
        }

        return false;
    }

    #region Ouline
    Transform StartOutline(Transform selected, bool active)
    {
        if (selected == null)
            return null;

        CharacterMovement characterMovement = selected.GetComponent<CharacterMovement>().GetInstance();
        
        GetGreenIdOutline(characterMovement.Value, active);

        if (characterMovement.StartOutline())
            return selected;
        else
            return null;
    }

    void StopOutline(Transform selected)
    {
        if (selected == null)
            return;

        CharacterMovement characterMovement = selected.GetComponent<CharacterMovement>().GetInstance();
        characterMovement.StopOutline();
    }


    public void GetGreenIdOutline(int value, bool active)
    {

        foreach (GameObject go in enemyCharacter)
        {
            if (go == null)
                return;
            PhotonView enemyView = go.GetComponent<PhotonView>();

            if (!enemyView.IsMine)
            {
                CharacterMovement character = go.GetComponent<CharacterMovement>();
                if (character == null)
                    return;

                if (character.Value < value && character.Value > 0)
                {
                    if (active)
                        character.EnemyOutline(Color.green, 3);
                    else
                        StopOutline(go.transform);
                }
                else if (character.Value > value && character.Value > 0)
                {
                    if (active)
                        character.EnemyOutline(Color.red, 3);
                    else
                        StopOutline(go.transform);
                }
                    
            }
        }
    }

    private void MoveCharacter(Vector3 position, int touchCount, string characterId, string status)
    {
        if(status == gameManager.GetStatus())
        {
            foreach(BaseLocation b in bases)
            {
                CharacterMovement charMovement = b.Character.GetComponent<CharacterMovement>();
                if(charMovement.GetCharacter().characterId == characterId)
                {
                    charMovement.Move(position, touchCount);
                }
            }
        }
        else
        {
            foreach(GameObject go in enemyCharacter)
            {

                if (go.name == (characterId + "(Clone)"))
                {
                    Debug.Log("Move : " + go.name);
                    CharacterMovement charMovement = go.GetComponent<CharacterMovement>();

                    charMovement.Move(position, touchCount);
                }
            }
        }
    }
    #endregion

    private IEnumerator ShowDrawTimer()
    {
        int timer = 10;

        while (timer >= 0 && requestDrawUI.activeInHierarchy)
        {
            requestTimeSlider.value = timer;
            timer--;
            yield return new WaitForSeconds(1f);
        }

        if(timer <= 0)
        {
            requestDrawUI.SetActive(false);
        }
    }

    #region Spawn
    [PunRPC]
    public void RoundResult(int roundResult)
    {
        if(roundResult != ENEMYLEFT)
            RoundEnded(roundResult);
        else
        {
            gameManager.MatchResult(1);
        }
    }

    [PunRPC]
    public void StorePrisoner(int countPrisoner)
    {
        countEnemyPrisoner = countPrisoner;
        CalculateResult();
    }

    [PunRPC]
    public void SetReady()
    {
        ready = !ready;
        CharacterInstantiatedEvent();
    }

    [PunRPC]
    public void MoveCharacterRPC(string characterId, string status, int touchCount, Vector3 position)
    {
        MoveCharacter(position, touchCount, characterId, status);
    }

    [PunRPC]
    public void RequestDraw()
    {
        requestDrawUI.SetActive(true);
        StartCoroutine(ShowDrawTimer());
    }

    [PunRPC]
    public void DrawMatch()
    {
        optionUi.SetActive(false);
        gameManager.MatchResult(DRAWMATCH);
    }

    #endregion

    public void CallRPCMethod(string methodName, string characterId, int cost, string playerType)
    {
        view.RPC(methodName, RpcTarget.Others, characterId, cost, playerType);
    }

    #region Button Method
    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void Option()
    {
        optionUi.SetActive(true);
    }

    public void ConfirmSurrend()
    {
        optionUi.SetActive(false);
        view.RPC("RoundResult", RpcTarget.Others, ENEMYLEFT);
        gameManager.MatchResult(ENEMYLEFT);
    }

    public void SelectSurrend()
    {
        requestSurrendUi.SetActive(true);
    }

    public void CancelSurrend()
    {
        requestSurrendUi.SetActive(false);
    }

    public void SelectDraw()
    {
        view.RPC("RequestDraw", RpcTarget.Others);
    }

    public void ConfirmDraw()
    {
        requestDrawUI.SetActive(false);
        view.RPC("DrawMatch", RpcTarget.All);
    }

    public void RefuseDraw()
    {
        requestDrawUI.SetActive(false);
    }

    public void CloseOption()
    {
        optionUi.SetActive(false);
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Debug.Log("Instantiate : " + info);
    }
    #endregion
}
