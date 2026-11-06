using System.Collections;
using TMPro;
using UnityEngine;

// Gắn vào một TextMeshPro trong Canvas, dùng để hiện thông báo ngắn (ví dụ nhặt vật phẩm)
public class NotificationUI : MonoBehaviour
{
    public static NotificationUI Instance { get; private set; }

    public float showDuration = 2.5f;

    private TextMeshProUGUI notificationText;
    private Coroutine hideCoroutine;

    private void Awake()
    {
        Instance = this;

        notificationText = GetComponent<TextMeshProUGUI>();
        notificationText.text = "";
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Show(string message)
    {
        notificationText.text = message;

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(showDuration);

        notificationText.text = "";
        hideCoroutine = null;
    }
}