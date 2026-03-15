using System.Collections.Generic;
using UnityEngine;

public class GlobalGameData : MonoBehaviour
{
    public static GlobalGameData Instance;

    public Character selectedCharacterData;
    public Character.Class selectedClass;

    [Header("Databáze itemù")]
    public List<Weapon> allAvailableWeapons;
    public List<Armor> allAvailableArmors;

    [Header("Inventáø")]
    public List<Weapon> playerWeapons = new List<Weapon>();
    public List<Armor> playerArmors = new List<Armor>();

    public int currentZoneLevel = 1;
    public int currentRound = 1;

    [System.Serializable]
    public class ZoneInfo
    {
        public string zoneName;
        public Sprite zoneBackground; 
    }

    public ZoneInfo[] allZones;

    // Singleton
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public Character GetSelectedCharacter()
    {
        return selectedCharacterData;
    }

    // Resetování postupu hráèe
    public void ResetCharacterData()
    {
        playerWeapons.Clear();
        playerArmors.Clear();

        currentZoneLevel = 1;
        currentRound = 1;

        if (selectedCharacterData != null)
        {
            selectedCharacterData.health = selectedCharacterData.maxHealth;

            if (selectedCharacterData.defaultWeapon != null)
            {
                selectedCharacterData.currentWeapon = selectedCharacterData.defaultWeapon;
            }

            if (selectedCharacterData.defaultArmor != null)
            {
                selectedCharacterData.currentArmor = selectedCharacterData.defaultArmor;
                selectedCharacterData.currentArmorValue = Random.Range(selectedCharacterData.defaultArmor.minArmorValue, selectedCharacterData.defaultArmor.maxArmorValue + 1);
            }
            else
            {
                selectedCharacterData.currentArmor = null;
                selectedCharacterData.currentArmorValue = 0;
            }
        }

    }
    // Získaní informací o stávajcí zónì
    public ZoneInfo GetCurrentZoneInfo()
    {
        int index = currentZoneLevel - 1;

        if (index >= 0 && index < allZones.Length)
        {
            return allZones[index];
        }

        return null;
    }
}

