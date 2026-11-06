using TMPro;
using UnityEngine;

// Gắn vào object CoinAmountText
public class CoinUI : MonoBehaviour
{
    private TextMeshProUGUI coinText;

    private void Awake()
    {
        coinText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        GameManager.Instance.OnCoinsChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCoinsChanged -= Refresh;
        }
    }

    private void Refresh()
    {
        coinText.text = GameManager.Instance.Coins.ToString();
    }
}