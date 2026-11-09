using System.Collections.Generic;
using UnityEngine;

// Lớp cha của mọi thứ phản ứng với Lightup: Lamp, Statue, LightReveal
// Mỗi vật tự đăng ký vào danh sách khi bật, Lightup chỉ cần gọi Burst
public abstract class LightReactive : MonoBehaviour
{
    private static readonly List<LightReactive> all = new List<LightReactive>();

    // Khi tắt Enter Play Mode Options thì danh sách static không tự xoá, nên xoá tay
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic()
    {
        all.Clear();
    }

    protected virtual void OnEnable()
    {
        all.Add(this);
    }

    protected virtual void OnDisable()
    {
        all.Remove(this);
    }

    // Mỗi loại tự quyết định làm gì khi bị chiếu sáng
    protected abstract void OnLit();

    // PlayerFight gọi lúc Lightup phát sáng
    public static void Burst(Vector2 center, float radius)
    {
        // Duyệt ngược vì một vật có thể tự gỡ khỏi danh sách khi bị chiếu sáng
        for (int i = all.Count - 1; i >= 0; i--)
        {
            LightReactive target = all[i];

            if (Vector2.Distance(center, target.transform.position) <= radius)
            {
                target.OnLit();
            }
        }
    }
}