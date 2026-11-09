using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// Cây đèn: tối cho đến khi người chơi dùng Lightup ở gần
public class Lamp : LightReactive
{
    // Statue lắng nghe để cập nhật số đèn đã thắp
    public static event Action<Lamp> OnAnyLampLit;

    // SPRITE (kéo sprite tắt và sprite sáng trong Lamp.aseprite vào)
    public SpriteRenderer spriteRenderer;
    public Sprite unlitSprite;
    public Sprite litSprite;

    // ÁNH SÁNG (Light2D con, tắt sẵn)
    public Light2D glow;

    public AudioClip litClip;

    public bool IsLit { get; private set; }

    private string id;

    private void Start()
    {
        // Mã đèn = tên màn + vị trí, giống mã xu
        Vector3 position = transform.position;

        id = SceneManager.GetActiveScene().name
            + "_lamp_" + Mathf.RoundToInt(position.x * 100f)
            + "_" + Mathf.RoundToInt(position.y * 100f);

        // Đèn đã thắp và lưu ở checkpoint trước thì sáng sẵn
        SetVisual(GameManager.Instance.IsLampLit(id));
    }

    protected override void OnLit()
    {
        if (IsLit)
        {
            return;
        }

        // Ghi vào dữ liệu hiện tại, chỉ được lưu thật khi thắp tượng (checkpoint)
        GameManager.Instance.LightLamp(id);

        SetVisual(true);

        if (litClip != null)
        {
            AudioSource.PlayClipAtPoint(litClip, transform.position);
        }

        OnAnyLampLit?.Invoke(this);
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
}