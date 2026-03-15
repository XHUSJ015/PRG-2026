using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Custom/Enemy")]
public class Enemy : ScriptableObject
{
    public int id;
    public string name;
    public int health;
    public int perception;
    public int agility;
    public int speed;

    public int minDamage;
    public int maxDamage;
    public int minArmor;                                                                            
    public int maxArmor;

    public int level;
    [HideInInspector]
    public int distance = 0;
    public int playerChanceToHit;   
    public int chanceToHitPlayer;
    public bool melee; 
    public Sprite enemySprite;
}
