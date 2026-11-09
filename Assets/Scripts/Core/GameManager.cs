using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    // SKILLS
    public void UnlockDash()
    {
        data.hasDash = true;
    }

    public void UnlockDoubleJump()
    {
        data.hasDoubleJump = true;
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

    // COINS
    public int Coins => data.coins;

    // UI (CoinUI) lắng nghe để cập nhật số xu
    public event System.Action OnCoinsChanged;

    public void AddCoins(int amount)
    {
        data.coins += amount;
        OnCoinsChanged?.Invoke();
    }

    public bool HasCollectedCoin(string id)
    {
        return data.collectedCoins.Contains(id);
    }

    public void CollectCoin(string id, int amount)
    {
        data.collectedCoins.Add(id);
        AddCoins(amount);
    }

    // LAMPS
    public bool IsLampLit(string id)
    {
        return data.litLamps.Contains(id);
    }

    public void LightLamp(string id)
    {
        if (!data.litLamps.Contains(id))
        {
            data.litLamps.Add(id);
        }
    }

    // CHECKPOINT
    // Chỉ nhận checkpoint có số thứ tự lớn hơn, để quay lại checkpoint cũ không làm lùi tiến trình
    public void SetCheckpoint(int order, Vector2 position)
    {
        string scene = SceneManager.GetActiveScene().name;

        if (data.checkpointScene == scene && order <= data.checkpointOrder)
        {
            return;
        }

        data.checkpointScene = scene;
        data.checkpointOrder = order;
        data.checkpointX = position.x;
        data.checkpointY = position.y;

        Save();
    }

    // Tượng nào có số thứ tự nhỏ hơn hoặc bằng checkpoint đã lưu thì đã sáng
    public bool HasReachedCheckpoint(int order)
    {
        return data.checkpointScene == SceneManager.GetActiveScene().name && data.checkpointOrder >= order;
    }

    public bool TryGetCheckpoint(out Vector2 position)
    {
        position = new Vector2(data.checkpointX, data.checkpointY);

        return data.checkpointScene == SceneManager.GetActiveScene().name;
    }

    // Chạm cờ qua màn: lưu tiến độ, xoá checkpoint và danh sách xu đã nhặt của màn này
    public void CompleteLevel(int levelReachedValue)
    {
        string scene = SceneManager.GetActiveScene().name;

        data.levelReached = Mathf.Max(data.levelReached, levelReachedValue);

        data.checkpointScene = "";
        data.checkpointOrder = 0;

        data.collectedCoins.RemoveAll(id => id.StartsWith(scene + "_"));
        data.litLamps.RemoveAll(id => id.StartsWith(scene + "_"));

        Save();
    }

    // Chết: nạp lại bản lưu gần nhất (xu, vật phẩm, kỹ năng nhặt sau checkpoint sẽ mất)
    public void RevertToCheckpoint()
    {
        bool hatOn = data.hatOn;
        bool capeOn = data.capeOn;
        string weapon = data.equippedWeapon;

        Load();

        // Giữ lựa chọn mặc đồ và vũ khí đang cầm nếu vẫn còn sở hữu
        data.hatOn = hatOn;
        data.capeOn = capeOn;

        if (HasItem(weapon))
        {
            data.equippedWeapon = weapon;
        }
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

        OnCoinsChanged?.Invoke();
    }

    // Chuột phải vào component GameManager lúc đang Play để xoá bản lưu khi test
    [ContextMenu("Reset Save")]
    public void ResetData()
    {
        data = new GameData();
        Save();

        OnCoinsChanged?.Invoke();
    }
}