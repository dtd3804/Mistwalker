using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    private Player player;
    private PlayerFight playerFight;

    // Tên object layer -> SpriteRenderer (không được đổi tên các layer trong Model)
    private Dictionary<string, SpriteRenderer> layers = new Dictionary<string, SpriteRenderer>();

    // UNITY LIFECYCLE
    void Start()
    {
        player = GetComponent<Player>();
        playerFight = GetComponent<PlayerFight>();

        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>(true))
        {
            layers[sr.gameObject.name] = sr;
        }
    }

    void Update()
    {
        HandleInput();
        HandleDebugInput();
    }

    // Chạy sau Animator để việc bật/tắt layer luôn là quyết định cuối cùng
    void LateUpdate()
    {
        ApplyVisibility();
    }

    // INPUT
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GameManager.Instance.ToggleWearable(ItemIds.HAT);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            GameManager.Instance.ToggleWearable(ItemIds.CAPE);
        }

        if (Input.GetKeyDown(KeyCode.Q) && CanSwitchWeapon())
        {
            GameManager.Instance.CycleWeapon();
        }
    }

    private bool CanSwitchWeapon()
    {
        return !playerFight.IsAttacking && !player.IsDashing && !player.IsDead;
    }

    // DEBUG (xoá khi làm xong cửa hàng): F1 mũ, F2 áo choàng, F3 bamboo, F4 lamppost
    private void HandleDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            DebugToggleItem(ItemIds.HAT);
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            DebugToggleItem(ItemIds.CAPE);
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            DebugToggleItem(ItemIds.BAMBOO);
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            DebugToggleItem(ItemIds.LAMPPOST);
        }
    }

    private void DebugToggleItem(string id)
    {
        if (GameManager.Instance.HasItem(id))
        {
            GameManager.Instance.RemoveItem(id);
        }
        else
        {
            GameManager.Instance.UnlockItem(id);
        }
    }

    // VISIBILITY
    private void ApplyVisibility()
    {
        GameManager gameManager = GameManager.Instance;

        bool hat = gameManager.IsWearing(ItemIds.HAT);
        bool cape = gameManager.IsWearing(ItemIds.CAPE);
        bool bamboo = gameManager.EquippedWeapon == ItemIds.BAMBOO;
        bool lamppost = gameManager.EquippedWeapon == ItemIds.LAMPPOST;

        SetLayer("Hat", hat);

        SetLayer("Cape_Front", cape);
        SetLayer("Cape_Back", cape);

        SetLayer("Bamboo", bamboo);
        SetLayer("Lamppost", lamppost);
    }

    private void SetLayer(string layerName, bool visible)
    {
        if (layers.TryGetValue(layerName, out SpriteRenderer sr))
        {
            sr.enabled = visible;
        }
    }
}