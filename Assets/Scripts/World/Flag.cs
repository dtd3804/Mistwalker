using UnityEngine;

public class Flag : MonoBehaviour
{
    public GameObject winUI;

    // Giá trị LevelReached mở khoá khi qua màn này (trước đây nằm ở NextLevel)
    public int levelReachedValue;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            GameManager.Instance.CompleteLevel(levelReachedValue);

            Time.timeScale = 0;
            winUI.SetActive(true);
        }
    }
}