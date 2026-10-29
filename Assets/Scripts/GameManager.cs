using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameData data = new GameData();

    private const string SAVE_KEY = "MistwalkerSave";

    // UNITY LIFECYCLE
    // Tự tạo GameManager trước khi scene đầu tiên chạy, không cần đặt tay vào scene
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance == null)
        {
            new GameObject("GameManager").AddComponent<GameManager>();
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    // ITEMS
    public string EquippedWeapon => data.equippedWeapon;
    public bool HasWeapon => !string.IsNullOrEmpty(data.equippedWeapon);

    public bool HasItem(string id)
    {
        return data.ownedItems.Contains(id);
    }

    public void UnlockItem(string id)
    {
        if (HasItem(id))
        {
            return;
        }

        data.ownedItems.Add(id);

        // Vũ khí đầu tiên nhận được sẽ tự động cầm
        if (System.Array.IndexOf(ItemIds.Weapons, id) >= 0 && !HasWeapon)
        {
            data.equippedWeapon = id;
        }
    }

    public void RemoveItem(string id)
    {
        data.ownedItems.Remove(id);

        if (data.equippedWeapon == id)
        {
            data.equippedWeapon = "";

            foreach (string weapon in ItemIds.Weapons)
            {
                if (HasItem(weapon))
                {
                    data.equippedWeapon = weapon;
                    break;
                }
            }
        }
    }

    // Mũ và áo choàng: phải sở hữu và đang mặc thì mới có hiệu lực
    public bool IsWearing(string id)
    {
        if (id == ItemIds.HAT)
        {
            return HasItem(id) && data.hatOn;
        }

        if (id == ItemIds.CAPE)
        {
            return HasItem(id) && data.capeOn;
        }

        return false;
    }

    public void ToggleWearable(string id)
    {
        if (!HasItem(id))
        {
            return;
        }

        if (id == ItemIds.HAT)
        {
            data.hatOn = !data.hatOn;
        }
        else if (id == ItemIds.CAPE)
        {
            data.capeOn = !data.capeOn;
        }
    }

    // Phím Q: chỉ có 0 hoặc 1 vũ khí thì không làm gì
    public void CycleWeapon()
    {
        List<string> owned = new List<string>();

        foreach (string weapon in ItemIds.Weapons)
        {
            if (HasItem(weapon))
            {
                owned.Add(weapon);
            }
        }

        if (owned.Count < 2)
        {
            return;
        }

        int current = owned.IndexOf(data.equippedWeapon);
        data.equippedWeapon = owned[(current + 1) % owned.Count];
    }

    // SAVE / LOAD
    public void Save()
    {
        PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public void Load()
    {
        string json = PlayerPrefs.GetString(SAVE_KEY, "");
        data = string.IsNullOrEmpty(json) ? new GameData() : JsonUtility.FromJson<GameData>(json);
    }

    public void ResetData()
    {
        data = new GameData();
        Save();
    }
}