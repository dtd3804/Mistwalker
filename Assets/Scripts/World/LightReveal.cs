using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

// Vật ẩn (bục nền, xu...) chỉ hiện và có va chạm trong vài giây sau khi bị Lightup chiếu
// Gắn vào object cha, các object con (sprite + collider) là phần bị ẩn. Object cha phải luôn bật
public class LightReveal : LightReactive
{
    public float revealDuration = 5f;

    // Nhấp nháy báo sắp biến mất trong khoảng thời gian này (tính trong revealDuration)
    public float blinkTime = 1f;

    public float fadeTime = 0.3f;

    private float alpha;
    private Coroutine routine;

    private void Awake()
    {
        SetSolid(false);
        SetAlpha(0f);
    }

    protected override void OnLit()
    {
        // Bị chiếu lại khi đang hiện thì tính lại từ đầu
        if (routine != null)
        {
            StopCoroutine(routine);
        }

        routine = StartCoroutine(RevealRoutine());
    }

    private IEnumerator RevealRoutine()
    {
        SetSolid(true);

        yield return FadeTo(1f);

        yield return new WaitForSeconds(Mathf.Max(0f, revealDuration - blinkTime));

        // Nhấp nháy báo sắp biến mất
        float timer = 0f;

        while (timer < blinkTime)
        {
            alpha = Mathf.Lerp(0.3f, 1f, Mathf.PingPong(timer * 6f, 1f));
            SetAlpha(alpha);

            timer += Time.deltaTime;
            yield return null;
        }

        // Tắt va chạm trước, rồi mờ dần
        SetSolid(false);

        yield return FadeTo(0f);

        routine = null;
    }

    private IEnumerator FadeTo(float target)
    {
        while (!Mathf.Approximately(alpha, target))
        {
            alpha = Mathf.MoveTowards(alpha, target, Time.deltaTime / fadeTime);
            SetAlpha(alpha);

            yield return null;
        }
    }

    private void SetSolid(bool solid)
    {
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>(true))
        {
            col.enabled = solid;
        }
    }

    private void SetAlpha(float value)
    {
        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>(true))
        {
            Color color = sr.color;
            color.a = value;
            sr.color = color;
        }

        foreach (Tilemap tilemap in GetComponentsInChildren<Tilemap>(true))
        {
            Color color = tilemap.color;
            color.a = value;
            tilemap.color = color;
        }
    }
}