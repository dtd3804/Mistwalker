using UnityEngine;

public class RestartGame : MonoBehaviour
{
    public void LoadCurrenScrene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        Time.timeScale = 1;
    }
}