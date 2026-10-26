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