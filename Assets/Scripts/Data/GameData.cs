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

    // COINS
    public int coins;
    public List<string> collectedCoins = new List<string>();

    // PROGRESS
    public int levelReached;

    // CHECKPOINT (rỗng nghĩa là chưa có checkpoint nào trong màn đang chơi)
    public string checkpointScene = "";
    public int checkpointOrder;
    public float checkpointX;
    public float checkpointY;
}