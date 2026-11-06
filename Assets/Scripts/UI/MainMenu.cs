using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public GameObject startMainMenu;
    public GameObject levelSelect;
    public void StartGame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        Time.timeScale = 1;
    }

    public void GoToLevelSelect()
    {
        startMainMenu.SetActive(false);
        levelSelect.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
