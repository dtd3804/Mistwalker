using UnityEngine;

public class JumpPad : MonoBehaviour
{
    // Độ cao nhảy cộng thêm cho cú nhảy kế tiếp của người chơi đang đứng trên tấm
    public float extraJumpHeight = 1.5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();

        if (player != null)
        {
            player.EnterJumpPad(this);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();

        if (player != null)
        {
            player.ExitJumpPad(this);
        }
    }
}