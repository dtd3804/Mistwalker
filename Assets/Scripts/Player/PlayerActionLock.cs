using UnityEngine;

public class PlayerActionLock : MonoBehaviour
{
    private float lockTimer;

    public bool IsLocked => lockTimer > 0f;

    private void Update()
    {
        if (lockTimer > 0f)
        {
            lockTimer -= Time.deltaTime;
        }
    }

    public void Lock(float duration)
    {
        lockTimer = Mathf.Max(lockTimer, duration);
    }
}