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

    public float horizontalResolution = 1920;

    void OnGUI()
    {
        float currentAspect = (float)Screen.width / (float)Screen.height;
        Camera.main.orthographicSize = horizontalResolution / currentAspect / 200;
    }

    public void SetCamera(string type)
    {
        if (type == "Player1")
            player1Cam.SetActive(true);
        else
            player2Cam.SetActive(true);
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
