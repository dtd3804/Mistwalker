using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public int level;
    void Start()
    {
        Button btn = GetComponent<Button>();

        if (GameManager.Instance.data.levelReached < level)
        {
            btn.interactable = false;
        }
    }

}