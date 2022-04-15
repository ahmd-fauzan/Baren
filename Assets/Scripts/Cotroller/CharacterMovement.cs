using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Photon.Realtime;
using System;

public class CharacterMovement : MonoBehaviour
{
    private NavMeshAgent agent;

    [SerializeField]
    private int value;

    GameManager gameManager;

    private Animator animator;

    [SerializeField]
    bool moving;

    PhotonView view;

    private Vector3 location;

    public bool IsRotating;

    public float RotationSpeed = 100f;

    [SerializeField]
    private int status;

    Rigidbody rigid;

    Character character;

    private int currStamina;

    private Slider staminaBar;

    public void SetCharacter(Character character, Slider staminaBar)
    {
        this.character = character;
        this.staminaBar = staminaBar;
        InitAgent();
        SetAttribute(character.walkSpeed, character.acceleration);
    }

    public int Status
    {
        get
        {
            return this.status;
        }
        set
        {
            this.status = value;
        }
    }
    public int Value
    {
        get
        {
            return this.value;
        }
        set
        {
            this.value = value;
        }
        
    }

    private void Start()
    {
        InitAgent();

        rigid = GetComponent<Rigidbody>();

        view = GetComponent<PhotonView>();

        animator = GetComponent<Animator>();

        if(view.IsMine)
            StartCoroutine(UpdateStamina());

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        moving = false;

        if(view.IsMine)
            currStamina = character.stamina;
    }

    public void InitAgent()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    public CharacterMovement GetInstance()
    {
        return gameObject.GetComponent<CharacterMovement>();
    }

    IEnumerator RotateAgent(Quaternion currentRotation, Quaternion targetRotation)
    {
        IsRotating = true;
        while (currentRotation != targetRotation) {

            transform.rotation = Quaternion.RotateTowards(currentRotation, targetRotation, 120f * Time.deltaTime);
            currentRotation = transform.rotation;
            yield return 1;
        }
        IsRotating = false;
        agent.enabled = true;
        
        agent.SetDestination(location);
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.Space))
            view.RPC("GetIdOutline", RpcTarget.Others, this.value, "show");

        if (Input.GetKeyUp(KeyCode.KeypadEnter))
            view.RPC("GetIdOutline", RpcTarget.Others, this.value, "stop");

        if(agent != null)
        {
            if (agent.enabled)
            {
                if (agent.remainingDistance > agent.stoppingDistance)
                {
                    animator.SetFloat("Moving", agent.speed);

                }
                else
                {
                    animator.SetFloat("Moving", -1f);

                    moving = false;
                }

                if (currStamina <= 0)
                {
                    agent.isStopped = true;

                    agent.ResetPath();
                }
                else
                    agent.isStopped = false;
            }
        }
        

        if (!moving)
        {
            
            rigid.constraints = RigidbodyConstraints.FreezeRotation;
        }
        else
        {
            
            rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    private IEnumerator UpdateStamina()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            if (moving)
            {
                if (agent.speed == character.walkSpeed && currStamina > 0)
                {
                    this.currStamina -= 1;
                }
                else if (agent.speed == character.runSpeed && currStamina > 0)
                {
                    this.currStamina -= 3;
                }
            }
            else
            {
                if (this.character.stamina != currStamina && currStamina < 100)
                {
                    this.currStamina += this.character.staminaRegen;
                }
            }

            staminaBar.value = this.currStamina;
        }
    }

    public void Move(Vector3 location, int touchCount)
    {
        if(currStamina > 0)
        {
            agent.enabled = false;

            moving = true;

            if (touchCount == 2)
                UpdateSpeed(character.runSpeed);
            else
                UpdateSpeed(character.walkSpeed);

            Vector3 direction = (location - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            StartCoroutine(RotateAgent(transform.rotation, lookRotation));
            this.location = location;
        }
        
    }

    public void SetAttribute(float speed, float acceleration)
    {
        agent.speed = speed;
        agent.acceleration = acceleration;
    }

    private void UpdateSpeed(float speed)
    {
        agent.speed = speed;
    }

    public bool StartOutline()
    {
        if (!view.IsMine)
            return false;

        Outline outline = GetComponent<Outline>();
        outline.OutlineColor = Color.white;
        outline.OutlineWidth = 3;

        //view.RPC("GetIdOutline", RpcTarget.All, this.value, "show");

        //view.RPC("TesMethod", RpcTarget.All, view.ViewID);

        return true;
    }

    public void StopOutline()
    {
        Outline outline = GetComponent<Outline>();
        outline.OutlineWidth = 0;

        //view.RPC("GetIdOutline", RpcTarget.All, this.value, "stop");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Character")
        {
            CharacterMovement character = collision.collider.GetComponent<CharacterMovement>();

            if (view.IsMine && !character.GetComponent<PhotonView>().IsMine)
            {
                if (character.value != -1)
                {
                    if (this.value < character.Value)
                    {
                        Move(gameManager.GetPrisoner(), 2);
                        status = -1;
                    }
                }
            }

            if (view.IsMine && character.GetComponent<PhotonView>().IsMine)
            {
                if (character.Value == -1 && this.Value != -1)
                    gameManager.ReleaseCharacter();
            }
        }

        if (gameManager.GetStatus() == "Player1")
        {
            if (collision.transform.tag == "EnemyFlag")
            {
                gameManager.RoundEnded(2);
                
            }
        }
        else if (!PhotonNetwork.IsMasterClient)
        {

            if (collision.transform.tag == "PlayerFlag")
            {
                gameManager.RoundEnded(2);
            }
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (other.transform.tag == "PlayerBaseLine" && view.IsMine)
            {
                gameManager.UpdateBaseLocation(this.transform);

                if (value == 0)
                    value = gameManager.AddValue();
                else
                    value = 0;
            }

            if (other.transform.tag == "EnemyBaseLine" && !view.IsMine)
            {
                gameManager.UpdateBaseLocation(this.transform);

                if (value == 0)
                    value = gameManager.AddValue();
                else
                    value = 0;
            }
        }
        else
        {
            if (other.transform.tag == "EnemyBaseLine" && view.IsMine)
            {
                gameManager.UpdateBaseLocation(this.transform);

                if (value == 0)
                    value = gameManager.AddValue();
                else
                    value = 0;
            }

            if (other.transform.tag == "PlayerBaseLine" && !view.IsMine)
            {
                gameManager.UpdateBaseLocation(this.transform);

                if (value == 0)
                    value = gameManager.AddValue();
                else
                    value = 0;
            }
        }
        
        if(other.transform.tag == "Mark")
        {
            Destroy(other.gameObject, 0.5f);
        }

        if(other.transform.tag == "Prisoner1" && gameManager.GetStatus() == "Player1" && this.status == -1)
        {
            this.value = -1;
        }

        if (other.transform.tag == "Prisoner2" && gameManager.GetStatus() == "Player2" && this.status == -1)
        {
            this.value = -1;
        }
    }

    public void EnemyOutline(Color color, int width)
    {
        Outline outline = GetComponent<Outline>();
        outline.OutlineColor = color;
        outline.OutlineWidth = width;
    }

    [PunRPC]
    public void ShowLose()
    {
        GameObject gameManager = GameObject.Find("GameManager");
        //gameManager.GetComponent<GameManager>().LoseReward();
    }
}
