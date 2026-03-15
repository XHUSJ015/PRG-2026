using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectManager : MonoBehaviour
{
    public Character[] allCharacters;
    private Character selectedCharacterData;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Character1Selected()
    {
        SetSelectedCharacter(Character.Class.Vojín);
    }
    public void Character2Selected()
    {
        SetSelectedCharacter(Character.Class.Kobra);
    }
    public void Character3Selected()
    {
        SetSelectedCharacter(Character.Class.Øezník);
    }

    // Provedení výbìru hráèovy postavy
    private void SetSelectedCharacter(Character.Class charClass)
    {
        foreach (Character characterData in allCharacters)
        {
            if (characterData.class_ == charClass)
            {
                selectedCharacterData = characterData;
                Debug.Log(characterData.name);
                break;
            }
        }
    }

    public void StartGame()
    {
        if (selectedCharacterData != null)
        {
            Debug.Log($"Vybrána postava: {selectedCharacterData.name}");
            GlobalGameData.Instance.selectedCharacterData = selectedCharacterData;
            GlobalGameData.Instance.selectedClass = selectedCharacterData.class_;
            GlobalGameData.Instance.ResetCharacterData();
            SceneManager.LoadScene("LoreIntroScene");
        }
        else
        {
            Debug.LogError("Vyberte postavu");
        }
    }
    public void GoBack()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
