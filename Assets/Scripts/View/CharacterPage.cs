using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterPage : MonoBehaviour
{
    [SerializeField]
    private GameObject attributBar;

    [SerializeField]
    private Slider runSpeedSlider;

    [SerializeField]
    private Slider walkSpeedSlider;

    [SerializeField]
    private Slider staminaSlider;

    [SerializeField]
    private Slider staminaRegenSlider;

    [SerializeField]
    private Slider accelerationSlider;

    [SerializeField]
    private Transform characterSpawn;

    private Character currentSelected;

    private const int MAXSTAMINA = 30;
    private const float MAXWALKSPEED = 0.7f;
    private const float MAXRUNSPEED = 3f;
    private const int MAXACCELERATION = 9;
    private const int MAXSTAMINAREGEN = 3;

    public void ShowAllCharacter()
    {
        CharacterController controller = GameObject.Find("CharacterController").GetComponent<CharacterController>();
        RectTransform rect = characterSpawn.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, 319 * 4);
        Character[] characterList = controller.GetCharacterList();

        for (int i = 0; i < characterList.Length; i++)
        {
            GameObject go = SpawnManager.SpawnCard(characterList[i], characterSpawn, false);
            AddEvent(go, characterList[i]);
        }
    }



    private void AddEvent(GameObject go, Character character)
    {
        if (go.GetComponent<EventTrigger>() == null)
        {
            go.AddComponent<EventTrigger>();
        }

        EventTrigger trigger = go.GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();

        entry.eventID = EventTriggerType.PointerClick;

        entry.callback.AddListener((functionIWant) => { ShowAttribut(character); });

        trigger.triggers.Add(entry);
    }

    private void ShowAttribut(Character character)
    {
        if (currentSelected != null)
        {
            if (currentSelected.characterId == character.characterId)
            {
                GameObject myEventSystem = GameObject.Find("EventSystem");
                myEventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(null);

                attributBar.SetActive(false);
                currentSelected = null;

                return;
            }
        }

        currentSelected = character;

        attributBar.SetActive(true);

        runSpeedSlider.maxValue = MAXRUNSPEED;
        walkSpeedSlider.maxValue = MAXWALKSPEED;
        staminaSlider.maxValue = MAXSTAMINA;
        staminaRegenSlider.maxValue = MAXSTAMINAREGEN;
        accelerationSlider.maxValue = MAXACCELERATION;

        runSpeedSlider.value = character.runSpeed;
        walkSpeedSlider.value = character.walkSpeed;
        staminaSlider.value = character.stamina;
        staminaRegenSlider.value = character.staminaRegen;
        accelerationSlider.value = character.acceleration;
    }
}
