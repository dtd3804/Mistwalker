using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// Tượng = checkpoint. Chỉ thắp được (bằng Lightup) khi mọi cây đèn trong danh sách đã sáng
public class Statue : LightReactive
{
    // Số thứ tự trong màn: 1, 2, 3... phải tăng dần theo đường đi của màn
    public int order = 1;

    // Chỗ người chơi xuất hiện lại sau khi chết (để trống thì dùng vị trí của tượng)
    public Transform respawnPoint;

    // Những cây đèn phải thắp trước (kéo thả từng đèn vào, mỗi tượng một danh sách riêng)
    public Lamp[] lamps;

    // SPRITE (kéo sprite tắt và sprite sáng trong Statue.aseprite vào)
    public SpriteRenderer spriteRenderer;
    public Sprite unlitSprite;
    public Sprite litSprite;

    // ÁNH SÁNG (Light2D con, tắt sẵn)
    public Light2D glow;

    public AudioClip litClip;

    public bool IsLit { get; private set; }

    private bool isChecking;

    protected override void OnEnable()
    {
        base.OnEnable();
        Lamp.OnAnyLampLit += HandleLampLit;
    }

    protected override void OnDisable()
    {
        Lamp.OnAnyLampLit -= HandleLampLit;
        base.OnDisable();
    }

    private void Start()
    {
        // Tượng đã thắp và lưu trước đó thì sáng sẵn
        SetVisual(GameManager.Instance.HasReachedCheckpoint(order));
    }

    protected override void OnLit()
    {
        if (IsLit || isChecking)
        {
            return;
        }

        StartCoroutine(TryLightRoutine());
    }

    private IEnumerator TryLightRoutine()
    {
        isChecking = true;

        // Chờ hết frame để những cây đèn bị chiếu sáng cùng lúc kịp sáng trước
        yield return null;

        isChecking = false;

        int remaining = CountUnlitLamps();

        if (remaining > 0)
        {
            Notify("Còn " + remaining + " ngọn đèn chưa thắp");
            yield break;
        }

        SetVisual(true);

        if (litClip != null)
        {
            AudioSource.PlayClipAtPoint(litClip, transform.position);
        }

        // Lưu tiến trình: xu, vật phẩm, đèn đã thắp đều được ghi lại ở đây
        Vector2 position = respawnPoint != null ? respawnPoint.position : transform.position;
        GameManager.Instance.SetCheckpoint(order, position);

        Notify("Đã lưu tiến trình");
    }

    // Mỗi lần một đèn của tượng này sáng thì báo tiến độ
    private void HandleLampLit(Lamp lamp)
    {
        if (IsLit || System.Array.IndexOf(lamps, lamp) < 0)
        {
            return;
        }

        Notify("Đã thắp " + (lamps.Length - CountUnlitLamps()) + "/" + lamps.Length + " ngọn đèn");
    }

    private int CountUnlitLamps()
    {
        int count = 0;

        foreach (Lamp lamp in lamps)
        {
            if (lamp != null && !lamp.IsLit)
            {
                count++;
            }
        }

        return count;
    }

    private void SetVisual(bool lit)
    {
        IsLit = lit;

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = lit ? litSprite : unlitSprite;
        }

        if (glow != null)
        {
            glow.enabled = lit;
        }
    }

    private void Notify(string message)
    {
        if (NotificationUI.Instance != null)
        {
            NotificationUI.Instance.Show(message);
        }
    }

    // Vẽ đường nối tới các đèn của tượng khi chọn tượng trong Scene
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        if (lamps == null)
        {
            return;
        }

        foreach (Lamp lamp in lamps)
        {
            if (lamp != null)
            {
                Gizmos.DrawLine(transform.position, lamp.transform.position);
            }
        }
    }
}