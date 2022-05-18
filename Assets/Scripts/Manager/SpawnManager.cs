using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Photon.Pun;
using UnityEngine.UI;

public class SpawnManager : MonoBehaviour
{
    public static GameObject PhotonSpawn(Character character, Vector3 spawnPoint, Quaternion rotation, Transform parent)
    {
        GameObject go = PhotonNetwork.Instantiate(Path.Combine("Prefab", character.characterId), spawnPoint, rotation);

        go.transform.SetParent(parent);

        return go;
    }

    public static GameObject LocalSpawn(GameObject prefab, Vector3 location)
    {
        GameObject go = Instantiate(prefab, location, Quaternion.identity);

        return go;
    }

    public static GameObject SpawnCard(Character character, Transform spawn, bool sliderActive)
    {
        GameObject go = Instantiate(character.characterImage, spawn.position, spawn.rotation);
        //go.GetComponent<Image>().sprite = sprites[i]; //Set the Sprite of the Image Component on the new GameObject
        RectTransform rect = go.GetComponent<RectTransform>();

        rect.SetParent(spawn.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel
        go.GetComponent<Transform>().localScale = Vector3.one;

        go.GetComponent<Transform>().GetChild(2).gameObject.SetActive(sliderActive);
        go.GetComponent<Transform>().GetChild(3).GetChild(0).gameObject.GetComponent<Text>().text = character.characterName;
        go.GetComponent<Transform>().GetChild(3).GetChild(1).gameObject.GetComponent<Text>().text = character.cost.ToString();

        return go;
    }
}
