using UnityEngine;

public class NextLevel : MonoBehaviour
{
    public string nextLevelName;
    public int nextLevelValue;
    public void LoadNextLevel()
    {
        PlayerPrefs.SetInt("LevelReached", nextLevelValue);
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextLevelName);
        Time.timeScale = 1;
    }
}
