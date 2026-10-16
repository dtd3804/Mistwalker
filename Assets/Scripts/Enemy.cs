using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 1f;

    [Header("Check")]
    public Transform groundCheck;
    public Transform wallCheck;

    public float groundDistance = 0.3f;
    public float wallDistance = 0.2f;

    public LayerMask groundLayer;

    private SpriteRenderer spriteRenderer;

    private int direction = -1;

    private bool isStunned;

    private Vector3 originalGroundCheckPosition;
    private Vector3 originalWallCheckPosition;

    private Coroutine blinkCoroutine;
    private Color originalColor;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.flipX = false;

        originalGroundCheckPosition =
            groundCheck.localPosition;

        originalWallCheckPosition =
            wallCheck.localPosition;

        originalColor = spriteRenderer.color;
    }

    private void Update()
    {
        if (isStunned)
        {
            return;
        }

        CheckObstacle();

        Move();
    }

    private void Move()
    {
        transform.position +=
            Vector3.right *
            direction *
            speed *
            Time.deltaTime;
    }

    private void CheckObstacle()
    {
        bool noGround =
            !Physics2D.Raycast(
                groundCheck.position,
                Vector2.down,
                groundDistance,
                groundLayer
            );

        bool hitWall =
            Physics2D.Raycast(
                wallCheck.position,
                Vector2.right * direction,
                wallDistance,
                groundLayer
            );

        if (noGround || hitWall)
        {
            Flip();
        }
    }

    private void Flip()
    {
        direction *= -1;

        spriteRenderer.flipX =
            direction > 0;
    }

    private void LateUpdate()
    {
        if (direction < 0)
        {
            groundCheck.localPosition =
                new Vector3(
                    -Mathf.Abs(originalGroundCheckPosition.x),
                    originalGroundCheckPosition.y,
                    originalGroundCheckPosition.z
                );

            wallCheck.localPosition =
                new Vector3(
                    -Mathf.Abs(originalWallCheckPosition.x),
                    originalWallCheckPosition.y,
                    originalWallCheckPosition.z
                );
        }
        else
        {
            groundCheck.localPosition =
                new Vector3(
                    Mathf.Abs(originalGroundCheckPosition.x),
                    originalGroundCheckPosition.y,
                    originalGroundCheckPosition.z
                );

            wallCheck.localPosition =
                new Vector3(
                    Mathf.Abs(originalWallCheckPosition.x),
                    originalWallCheckPosition.y,
                    originalWallCheckPosition.z
                );
        }
    }

    public void TakeHit()
    {
        StartCoroutine(HitRoutine());

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }

        blinkCoroutine = StartCoroutine(BlinkRed());
    }

    private IEnumerator HitRoutine()
    {
        isStunned = true;

        yield return new WaitForSeconds(0.15f);

        isStunned = false;
    }
    private IEnumerator BlinkRed()
    {
        spriteRenderer.color = Color.pink;

        yield return new WaitForSeconds(0.1f);

        spriteRenderer.color = originalColor;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawLine(
                groundCheck.position,
                groundCheck.position +
                Vector3.down * groundDistance
            );
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.red;

            Vector3 dir =
                Application.isPlaying
                ? Vector3.right * direction
                : Vector3.left;

            Gizmos.DrawLine(
                wallCheck.position,
                wallCheck.position +
                dir * wallDistance
            );
        }
    }
}