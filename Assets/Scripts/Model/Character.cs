using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "CreateCharacter", order = 1)]
public class Character : ScriptableObject
{
    public string characterId;
    public string characterName;
    public GameObject characterImage;
    public GameObject characterPrefab;
    public float walkSpeed;
    public float runSpeed;
    public float acceleration;
    public int stamina;
    public int staminaRegen;
    public int cost;
}
