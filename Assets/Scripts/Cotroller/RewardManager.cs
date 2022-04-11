using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class RewardManager : MonoBehaviour
{
    //private static int playerScore;
   // private static int playerPoint;
    //private text playerScoreText;
    //private text enemyScoreText;
    //private text playerPointText;
    //private text enemyPointText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadScene(){
        PhotonNetwork.LoadLevel("Menu");
    }

    //public void SetScoreText(){
    //    playerScoreText.text = playerScore.ToString();
    //    enemyScoreText.text = enemyScore.ToString();
    //}

    // public void SetPointText(){
    //    playerPointText.text = playerPoint.ToString();
    //    enemyPointText.text = enemyPoint.ToString();
    //}
}
