using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private GameObject player1Cam;

    [SerializeField]
    private GameObject player2Cam;

    [SerializeField]
    private GameObject player1Light;

    [SerializeField]
    private GameObject player2Light;

    public float horizontalResolution = 1920;

    void OnGUI()
    {
        float currentAspect = (float)Screen.width / (float)Screen.height;
        Camera.main.orthographicSize = horizontalResolution / currentAspect / 200;
    }

    public void SetCamera(string type)
    {
        player1Cam.SetActive(type == "Player1");
        player2Cam.SetActive(type == "Player2");

        player1Light.SetActive(type == "Player1");
        player2Light.SetActive(type == "Player2");
    }

    
    public Camera GetCamera(string type)
    {
        if (!player1Cam.activeInHierarchy && !player2Cam.activeInHierarchy)
            SetCamera(type);

        if (player1Cam.activeInHierarchy)
            return player1Cam.GetComponent<Camera>();
        else
            return player2Cam.GetComponent<Camera>();

    }
}
