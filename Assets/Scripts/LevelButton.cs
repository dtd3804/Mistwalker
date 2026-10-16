using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public int level;
    void Start()
    {
        Button btn = GetComponent<Button>();

        if(PlayerPrefs.GetInt("LevelReached") < level)
        {
            btn.interactable = false;
        }
    }

}
