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

    RoundManager roundManager;

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

    public Character GetCharacter()
    {
        return this.character;
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

        roundManager = GameObject.Find("RoundManager").GetComponent<RoundManager>();
        
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
        //agent.enabled = true;
        
        agent.SetDestination(location);
    }

    private void LookCharacter(Vector3 target)
    {
        transform.LookAt(target);
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

                    /*
                    if(this.value == -1)
                    {
                        rigid.constraints = RigidbodyConstraints.FreezeAll;
                    }*/
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
            //agent.enabled = false;

            moving = true;

            if (touchCount == 2)
                UpdateSpeed(character.runSpeed);
            else
                UpdateSpeed(character.walkSpeed);

            LookCharacter(location);

            agent.SetDestination(location);

            //Vector3 direction = (location - transform.position).normalized;
            //Quaternion lookRotation = Quaternion.LookRotation(direction);

            //StartCoroutine(RotateAgent(transform.rotation, lookRotation));
            //this.location = location;
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

        return true;
    }

    public void StopOutline()
    {
        Outline outline = GetComponent<Outline>();
        outline.OutlineWidth = 0;
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
                        Move(roundManager.GetPrisoner(), 2);
                        this.value = -1;
                        StopOutline();
                    }
                }
            }

            if (view.IsMine && character.GetComponent<PhotonView>().IsMine)
            {
                if (character.Value == -1 && this.Value != -1)
                    roundManager.ReleaseCharacter();
            }
        }

        if (collision.transform.tag == "EnemyFlag")
        {
            if (view.IsMine && gameManager.GetStatus() == "Player1")
            {
                roundManager.SetRound(2);
            }
        }
        
        if(collision.transform.tag == "PlayerFlag")
        {
            if (view.IsMine && gameManager.GetStatus() == "Player2")
            {
                roundManager.SetRound(2);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (gameManager.GetStatus() == "Player1")
        {
            if (other.transform.tag == "PlayerBaseLine" && view.IsMine)
            {
                if (value == 0)
                    value = roundManager.AddValue();
                else
                    value = 0;

                roundManager.UpdateBaseLocation(this.transform);
            }

            if (other.transform.tag == "EnemyBaseLine" && !view.IsMine)
            {
                if (value == 0)
                {
                    value = roundManager.AddValue();
                    roundManager.StartOneOutline(this.GetInstance());
                }
                else
                {
                    value = 0;
                    StopOutline();
                }
            }
        }
        else
        {
            if (other.transform.tag == "EnemyBaseLine" && view.IsMine)
            {
                if (value == 0)
                    value = roundManager.AddValue();
                else
                    value = 0;

                roundManager.UpdateBaseLocation(this.transform);

            }

            if (other.transform.tag == "PlayerBaseLine" && !view.IsMine)
            {
                if (value == 0)
                {
                    value = roundManager.AddValue();
                    roundManager.StartOneOutline(this.GetInstance());
                }
                else
                {
                    value = 0;
                    StopOutline();
                }


            }
        }
        
        if(other.transform.tag == "Mark")
        {
            Destroy(other.gameObject, 0.5f);
        }

        /*
        if(other.transform.tag == "Prisoner1" && gameManager.GetStatus() == "Player1" && this.status == -1)
        {
            this.value = -1;
        }

        if (other.transform.tag == "Prisoner2" && gameManager.GetStatus() == "Player2" && this.status == -1)
        {
            this.value = -1;
        }*/
    }

    public void EnemyOutline(Color color, int width)
    {
        Outline outline = GetComponent<Outline>();
        outline.OutlineColor = color;
        outline.OutlineWidth = width;
    }
}
