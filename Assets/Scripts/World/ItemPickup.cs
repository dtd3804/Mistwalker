using UnityEngine;

public enum PickupType { Bamboo, Lamppost, Dash, DoubleJump }

public class ItemPickup : MonoBehaviour
{
    public PickupType type;

    [Tooltip("Để trống thì dùng thông báo mặc định theo loại vật phẩm")]
    public string message;

    public AudioClip pickupClip;

    private bool collected;

    private void Start()
    {
        // Đã sở hữu (đã lưu ở checkpoint trước đó) thì không xuất hiện lại
        if (IsOwned())
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

        Unlock();

        if (pickupClip != null)
        {
            player.PlaySFX(pickupClip);
        }

        if (NotificationUI.Instance != null)
        {
            NotificationUI.Instance.Show(string.IsNullOrEmpty(message) ? GetDefaultMessage() : message);
        }

        Destroy(gameObject);
    }

    private bool IsOwned()
    {
        GameManager gameManager = GameManager.Instance;

        switch (type)
        {
            case PickupType.Bamboo:
                return gameManager.HasItem(ItemIds.BAMBOO);

            case PickupType.Lamppost:
                return gameManager.HasItem(ItemIds.LAMPPOST);

            case PickupType.Dash:
                return gameManager.data.hasDash;

            case PickupType.DoubleJump:
                return gameManager.data.hasDoubleJump;
        }

        return false;
    }

    // Chỉ ghi vào dữ liệu đang chơi, được lưu khi chạm checkpoint hoặc qua màn
    private void Unlock()
    {
        GameManager gameManager = GameManager.Instance;

        switch (type)
        {
            case PickupType.Bamboo:
                gameManager.UnlockItem(ItemIds.BAMBOO);
                break;

            case PickupType.Lamppost:
                gameManager.UnlockItem(ItemIds.LAMPPOST);
                break;

            case PickupType.Dash:
                gameManager.UnlockDash();
                break;

            case PickupType.DoubleJump:
                gameManager.UnlockDoubleJump();
                break;
        }
    }

    private string GetDefaultMessage()
    {
        switch (type)
        {
            case PickupType.Bamboo:
                return "Nhặt được Bamboo";

            case PickupType.Lamppost:
                return "Nhặt được Lamppost (phím F: Lightup)";

            case PickupType.Dash:
                return "Học được Lướt (chuột phải)";

            case PickupType.DoubleJump:
                return "Học được Nhảy đôi";
        }

        return "";
    }
}