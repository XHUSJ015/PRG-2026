using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class InventoryItemSlot : MonoBehaviour, IPointerClickHandler
{
    private Weapon weaponData;
    private Armor armorData;
    private bool isWeapon;

    private InventoryManager inventoryManager;

    public TextMeshProUGUI nameText;

    // Složení itemslotu pro zbraò
    public void SetupWeapon(Weapon weapon, InventoryManager manager)
    {
        this.weaponData = weapon;
        this.inventoryManager = manager;
        this.isWeapon = true;
        this.nameText.text = weapon.name;
    }

    // Složení itemslotu pro zbroj
    public void SetupArmor(Armor armor, InventoryManager manager)
    {
        this.armorData = armor;
        this.inventoryManager = manager;
        this.isWeapon = false;
        this.nameText.text = armor.name;
    }

    // Logika pøi interakci s itemslotem
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isWeapon)
        {
            inventoryManager.ShowWeaponStats(weaponData);

            if (eventData.clickCount == 2)
            {
                inventoryManager.TryEquipWeapon(weaponData);
            }
        }
        else
        {
            inventoryManager.ShowArmorStats(armorData);

            if (eventData.clickCount == 2)
            {
                inventoryManager.TryEquipArmor(armorData);
            }
        }
    }
}