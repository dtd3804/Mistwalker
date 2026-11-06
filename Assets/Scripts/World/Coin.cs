using UnityEngine;
using UnityEngine.SceneManagement;

public class Coin : MonoBehaviour
{
    public AudioClip coinClip;
    public int coinsToGive = 1;

    private string id;
    private bool collected;

    private void Start()
    {
        // Mã xu = tên màn + vị trí, không cần đặt tay. Đổi vị trí xu trong editor thì mã đổi theo
        Vector3 position = transform.position;

        id = SceneManager.GetActiveScene().name
            + "_" + Mathf.RoundToInt(position.x * 100f)
            + "_" + Mathf.RoundToInt(position.y * 100f);

        // Đã nhặt và lưu ở checkpoint trước đó thì không xuất hiện lại
        if (GameManager.Instance.HasCollectedCoin(id))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();

        if (player == null || collected)
        {
            return;
        }

        collected = true;

        GameManager.Instance.CollectCoin(id, coinsToGive);
        player.PlaySFX(coinClip, 0.4f);

        Destroy(gameObject);
    }
}