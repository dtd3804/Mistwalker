using System.Collections.Generic;

public static class ItemIds
{
    public const string HAT = "hat";
    public const string CAPE = "cape";
    public const string BAMBOO = "bamboo";
    public const string LAMPPOST = "lamppost";

    // Thứ tự đổi vũ khí khi bấm Q
    public static readonly string[] Weapons = { BAMBOO, LAMPPOST };
}

[System.Serializable]
public class GameData
{
    // ITEMS
    public List<string> ownedItems = new List<string>();
    public string equippedWeapon = "";
    public bool hatOn = true;
    public bool capeOn = true;

    // SKILLS
    public bool hasDash;
    public bool hasDoubleJump;
}