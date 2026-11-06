using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    // Số thứ tự trong màn: 1, 2, 3... phải tăng dần theo đường đi của màn
    public int order = 1;

    // Chỗ người chơi xuất hiện lại sau khi chết (để trống thì dùng vị trí của checkpoint)
    public Transform respawnPoint;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();

        if (player == null)
        {
            return;
        }

        Vector2 position = respawnPoint != null ? respawnPoint.position : transform.position;

        GameManager.Instance.SetCheckpoint(order, position);
    }
}