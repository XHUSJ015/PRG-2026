using UnityEngine;

[CreateAssetMenu(fileName = "Armor", menuName = "Custom/Armor")]
public class Armor : ScriptableObject
{
    public int minArmorValue;
    public int maxArmorValue;
    public int level;
    public string description;

    public Armor(int minArmorValue, int maxArmorValue, int level, string description)
    {
        this.minArmorValue = minArmorValue;
        this.maxArmorValue = maxArmorValue;
        this.level = level;
        this.description = description;
    }
    public override string ToString()
    {
        return name;
    }
}
