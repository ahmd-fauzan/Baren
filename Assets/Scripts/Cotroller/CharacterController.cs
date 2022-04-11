using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [SerializeField]
    private Character[] characterList;

    private void Awake()
    {
        characterList = Resources.LoadAll<Character>("CharacterData");
        DontDestroyOnLoad(this.gameObject);
    }

    public Character[] GetCharacterList()
    {
        return characterList;
    }

   public Character GetCharacterById(string characterId)
    {
        for(int i = 0; i < characterList.Length; i++)
        {
            if (characterList[i].characterId == characterId)
                return characterList[i];
        }

        return null;
    }
}
