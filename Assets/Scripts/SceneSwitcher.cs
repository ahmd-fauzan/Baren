using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public GameObject PlayerProfileDetail;
    public DatabaseManager DatabaseManagerScript;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayerNameClicked()
    {
        PlayerProfileDetail.SetActive(true);
        DatabaseManagerScript.getStatistic();
        DatabaseManagerScript.setBattlePointAndUsernameTXT();
        DatabaseManagerScript.setUserIDTXT();
    }

    public void closeDetailPlayer()
    {
        PlayerProfileDetail.SetActive(false);

    }





}
