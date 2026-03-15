using System.Collections.Generic;
using UnityEditor.Playables;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Custom/Character")]
public class Character : ScriptableObject
{
    public Class class_;
    public int health = 100;
    public int maxHealth = 100;
    public int perception;
    public int agility;
    public int speed;
    public Weapon defaultWeapon;
    public Armor defaultArmor;
    public Weapon currentWeapon;
    public Armor currentArmor;
    public int currentArmorValue;
    public int level;
    public int round;

    public Sprite sprite;
    public string specialAttackName; 
    public string specialAbilityName;

    public enum Class
    {
        Vojín = 1,
        Kobra = 2,
        Øezník = 3,
    }
}
