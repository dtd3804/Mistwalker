using UnityEngine;
using TMPro;

public class Coin : MonoBehaviour
{
    public AudioClip coinClip;
    public int coinsToGive = 1;
    private TextMeshProUGUI coinText;

    private void Start()
    {
        coinText = GameObject.FindWithTag("CoinText").GetComponent<TextMeshProUGUI>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            Player player = collision.gameObject.GetComponent<Player>();
            player.coins += coinsToGive;
            player.PlaySFX(coinClip, 0.4f);
            coinText.text = player.coins.ToString();
            Destroy(gameObject);
        }
    }
}
