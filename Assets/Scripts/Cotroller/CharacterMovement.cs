using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Realtime;
using System;

public class CharacterMovement : MonoBehaviour
{
    private NavMeshAgent agent;

    [SerializeField]
    private int stamina;

    [SerializeField]
    private string charName;

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
    void Start()
    {
        
        agent = GetComponent<NavMeshAgent>();
        //agent.updateRotation = false;

        animator = GetComponent<Animator>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        view = GetComponent<PhotonView>();

        moving = false;
        //agent.isStopped = true;

        Debug.Log("Client : " + PhotonNetwork.IsMasterClient);
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

        /*if (agent.remainingDistance < agent.stoppingDistance)
        {
        
                moving = false;
        }*/
            

        if (!moving)
        {
            StartAnimation(true);
        }
        else
            StartAnimation(false);
    }

    private void StartAnimation(bool idle)
    {
        if (!idle)
        {
            if (stamina < 10)
            {
                ChangeAnimation("IdleToWalk");
            }
            else
                ChangeAnimation("IdleToRun");
        }
        else
            if (stamina < 10)
                ChangeAnimation("WalkToIdle");
            else
                ChangeAnimation("RunToIdle");
        
    }

    private void ChangeAnimation(string type)
    {
        switch (type)
        {
            case "IdleToRun":
                animator.SetBool("IsRun", true);

                break;
            case "RunToIdle":
                animator.SetBool("IsRun", false);
                break;
            case "IdleToWalk":
                animator.SetBool("IsWalk", true);

                break;
            case "WalkToIdle":
                animator.SetBool("IsWalk", false);
                break;
            case "WalkToRun":
                animator.SetBool("IsRun", false);
                animator.SetBool("IsWalk", true);
                break;
            case "RunToWalk":
                animator.SetBool("IsRun", true);
                animator.SetBool("IsWalk", false);
                break;
            default:
                // code block
                break;
        }
    }

    public void Move(Vector3 location)
    {
        agent.enabled = false;

        Vector3 direction = (location - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        StartCoroutine(RotateAgent(transform.rotation, lookRotation));
        this.location = location;

        if(!IsRotating)
            
        //StartCoroutine(RotateToNextPosition());
        moving = true;
    }

    public void SetAttribute(float speed, float acceleration, int stamina, string charName, int value)
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        agent.acceleration = acceleration;
        this.stamina = stamina;
        this.name = charName;
        this.value = value;
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

            Debug.Log("Touching");
            if (view.IsMine)
            {
                if (character.value != -1)
                {
                    if (this.value < character.value)
                    {
                        Move(gameManager.GetPrisoner());
                        status = -1;
                    }
                }

                if (character.GetComponent<PhotonView>().IsMine)
                {
                    if (character.Value == -1 && this.value != -1)
                        gameManager.ReleaseCharacter();
                }
            }
            

            

        }

        if (PhotonNetwork.IsMasterClient)
        {
            if (collision.transform.tag == "EnemyFlag")
            {
                GameObject gameManager = GameObject.Find("GameManager");
                //gameManager.GetComponent<GameManager>().Reward();

                view.RPC("ShowLose", RpcTarget.Others);
            }
        }
        else if (!PhotonNetwork.IsMasterClient)
        {

            if (collision.transform.tag == "PlayerFlag")
            {
                GameObject gameManager = GameObject.Find("GameManager");
                //gameManager.GetComponent<GameManager>().Reward();

                view.RPC("ShowLose", RpcTarget.Others);
            }
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (other.transform.tag == "PlayerBaseLine" && view.IsMine)
            {
                Debug.Log("BaseLine");
                if (view.IsMine)
                {
                    gameManager.UpdateBaseLocation(this.transform);

                    if (value == 0)
                        value = gameManager.AddValue();
                    else
                        value = 0;

                    
                }
            }
            if (other.transform.tag == "EnemyBaseLine")
            {
                gameManager.UpdateBaseLocation(this.transform);

                if (!view.IsMine)
                {
                    if (value == 0)
                        value = gameManager.AddValue();
                    else
                        value = 0;
                    
                }
            }
        }
        else
        {
            if (other.transform.tag == "EnemyBaseLine")
            {
                gameManager.UpdateBaseLocation(this.transform);

                if (view.IsMine)
                {
                    if (value == 0)
                        value = gameManager.AddValue();
                    else
                        value = 0;
                    
                }
            }

            if (other.transform.tag == "PlayerBaseLine")
            {
                gameManager.UpdateBaseLocation(this.transform);

                if (!view.IsMine)
                {
                    if (value == 0)
                        value = gameManager.AddValue();
                    else
                        value = 0;
                    
                }
            }
        }
        
        if(other.transform.tag == "Mark")
        {
            Destroy(other.gameObject, 0.5f);
        }

        if(other.transform.tag == "Prisoner1" && PhotonNetwork.IsMasterClient && this.status == -1)
        {
            this.value = -1;
        }

        if (other.transform.tag == "Prisoner2" && !PhotonNetwork.IsMasterClient && this.status == -1)
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

    /*
    [PunRPC]
    public void RedOutline(int idIntance)
    {
        Debug.Log(view.GetInstanceID() + " : " + idIntance);
        if (view.ViewID == idIntance)
            EnemyOutline(Color.red, 3);
    }

    [PunRPC]
    public void GreenOutline(int idIntance)
    {
        Debug.Log(view.GetInstanceID() + " : " + idIntance);
        if (view.ViewID == idIntance)
            EnemyOutline(Color.green, 3);
    }

    [PunRPC]
    public void GetIdOutline(int value, string type)
    {
        Debug.Log("Parameter Value : " + value + " Type : " + type);

        Debug.Log("My ID : " + view.ViewID);

        if(type == "show")
        {
            if (view.IsMine && value > this.value)
                view.RPC("GreenOutline", RpcTarget.All, view.ViewID);
            if (view.IsMine && value < this.value)
                view.RPC("RedOutline", RpcTarget.All, view.ViewID);
        }
        else
        {
            if (view.IsMine && (value > this.value || value < this.value))
                HideOutline(view.ViewID);
        }

        
    }

    [PunRPC]
    public void HideOutline(int idView)
    {
        if (view.ViewID == idView)
            EnemyOutline(Color.white, 0);
    }

    [PunRPC]
    public void TesMethod(int viewId)
    {
        view.RPC("Tes2", RpcTarget.All, viewId + 100);
        Debug.Log("Before : " + viewId);
    }

    [PunRPC]
    public void Tes2(int nilai)
    {
        Debug.Log("Received : " + nilai);
    }

    */
    [PunRPC]
    public void ShowLose()
    {
        GameObject gameManager = GameObject.Find("GameManager");
        //gameManager.GetComponent<GameManager>().LoseReward();
    }
}
