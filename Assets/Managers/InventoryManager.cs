using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("Zbranì")]
    public TextMeshProUGUI equippedWeaponName;
    public TextMeshProUGUI weaponStatsText;
    public Transform weaponListContent;

    [Header("Zbroj")]
    public TextMeshProUGUI equippedArmorName;
    public TextMeshProUGUI armorStatsText;
    public Transform armorListContent;

    [Header("Ostatní")]
    public GameObject itemSlotPrefab;
    public GameObject inventoryPanel;

    public GameObject openInventoryButton;
    public GameObject characterCard;

    private BattleManager battleManager;

    void Start()
    {
        battleManager = FindObjectOfType<BattleManager>();
        inventoryPanel.SetActive(false);
    }

    public void OpenInventory()
    {
        inventoryPanel.SetActive(true);

        if (openInventoryButton != null)
            openInventoryButton.SetActive(false);
            characterCard.SetActive(false);

        RefreshUI();
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);

        if (openInventoryButton != null)
            openInventoryButton.SetActive(true);
            characterCard.SetActive(true);
    }

    // Obnovení UI inventáøe
    public void RefreshUI()
    {
        Character player = GlobalGameData.Instance.GetSelectedCharacter();

        if (player.currentWeapon != null)
        {
            equippedWeaponName.text = player.currentWeapon.name;

            ShowWeaponStats(player.currentWeapon);
        }
        else
        {
            equippedWeaponName.text = "Žádná zbraò";
            weaponStatsText.text = "Bojuješ holýma rukama";
        }

        if (player.currentArmor != null)
        {
            equippedArmorName.text = player.currentArmor.name;

            ShowArmorStats(player.currentArmor);
        }
        else
        {
            equippedArmorName.text = "Žádná zbroj";
            armorStatsText.text = "Jsi bez brnìní";
        }

        foreach (Transform child in weaponListContent) Destroy(child.gameObject);
        foreach (Transform child in armorListContent) Destroy(child.gameObject);

        foreach (Weapon w in GlobalGameData.Instance.playerWeapons)
        {
            GameObject slot = Instantiate(itemSlotPrefab, weaponListContent);
            slot.GetComponent<InventoryItemSlot>().SetupWeapon(w, this);
        }

        foreach (Armor a in GlobalGameData.Instance.playerArmors)
        {
            GameObject slot = Instantiate(itemSlotPrefab, armorListContent);
            slot.GetComponent<InventoryItemSlot>().SetupArmor(a, this);
        }
    }

    // Zobrazení statistik zbranì
    public void ShowWeaponStats(Weapon w)
    {
        weaponStatsText.text = $"<b>{w.name}</b>\n\n" +
                               $"Poškození: {w.minDamage}-{w.maxDamage}\n" +
                               $"Pøesnost: {w.accuracy}";
    }

    // Zobrazení statistik zbroje
    public void ShowArmorStats(Armor a)
    {
        armorStatsText.text = $"<b>{a.name}</b>\n\n" +
                              $"Obrana: {a.minArmorValue}-{a.maxArmorValue}";
    }

    // Vybavení zbraòe
    public void TryEquipWeapon(Weapon newWeapon)
    {
        bool isBattleOver = battleManager.currentState == BattleManager.BattleState.BattleEnd;

        if (!isBattleOver && battleManager.playerActionPoints < 1)
        {
            Debug.Log("Nedostatek AP");
            return;
        }

        Character player = GlobalGameData.Instance.GetSelectedCharacter();
        Weapon oldWeapon = player.currentWeapon;

        if (oldWeapon != null)
        {
            GlobalGameData.Instance.playerWeapons.Add(oldWeapon);
        }

        player.currentWeapon = newWeapon;
        GlobalGameData.Instance.playerWeapons.Remove(newWeapon);
        if (!isBattleOver) battleManager.PayActionPoints(1);

        RefreshUI();
        battleManager.RefreshPlayerCard();

        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        foreach (EnemyController enemy in enemies)
        {
            enemy.UpdateHitChanceUI();
        }
    }

    // Vybavení zbroje
    public void TryEquipArmor(Armor newArmor)
    {
        bool isBattleOver = battleManager.currentState == BattleManager.BattleState.BattleEnd;

        if (battleManager.playerActionPoints < 1)
        {
            Debug.Log("Nedostatek AP");
            return;
        }

        Character player = GlobalGameData.Instance.GetSelectedCharacter();
        Armor oldArmor = player.currentArmor;

        if (oldArmor != null)
        {
            GlobalGameData.Instance.playerArmors.Add(oldArmor);
        }

        player.currentArmor = newArmor;
        player.currentArmorValue = Random.Range(newArmor.minArmorValue, newArmor.maxArmorValue + 1);

        GlobalGameData.Instance.playerArmors.Remove(newArmor);
        if (!isBattleOver) battleManager.PayActionPoints(1);

        RefreshUI();
        battleManager.RefreshPlayerCard();
    }
}