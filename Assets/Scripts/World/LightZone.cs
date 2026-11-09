using UnityEngine;

// Vùng ánh sáng: người chơi bước vào thì Global Light đổi sang màu và độ sáng của vùng này
// Gắn cùng một BoxCollider2D (Is Trigger). Dùng cho: sương mù cuối màn 1, hầm ngục tối...
[RequireComponent(typeof(Collider2D))]
public class LightZone : MonoBehaviour
{
    public GlobalLightFader globalLight;
    public Color color = Color.white;
    public float intensity = 1f;
    public float fadeTime = 1.5f;

    private void Start()
    {
        // Hồi sinh ngay trong vùng này thì áp dụng luôn, không đợi bước vào lần nữa
        Player player = FindFirstObjectByType<Player>();

        if (player != null && GetComponent<Collider2D>().OverlapPoint(player.transform.position))
        {
            globalLight.FadeTo(color, intensity, 0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null)
        {
            globalLight.FadeTo(color, intensity, fadeTime);
        }
    }
}