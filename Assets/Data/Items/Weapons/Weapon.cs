using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Custom/Weapon")]
public class Weapon : ScriptableObject
{
    public int minDamage;
    public int maxDamage;
    public int accuracy;
    public int level;
    public string description;

    public Weapon(int minDamage, int maxDamage, int accuracy, int level, string description)
    {
        this.minDamage = minDamage;
        this.maxDamage = maxDamage;
        this.accuracy = accuracy;
        this.level = level;
        this.description = description;
    }
    public override string ToString()
    {
        return name;
    }
}
