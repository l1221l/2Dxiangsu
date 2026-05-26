using UnityEngine;

/// <summary>
/// 楼层传送器 - 用于在楼层间移动
/// </summary>
public class FloorTeleport : MonoBehaviour
{
    [Header("传送设置")]
    public TeleportDirection Direction = TeleportDirection.Up;
    public int TargetFloor = 1;
    public string TargetSpawnPointName = "SpawnPoint_Floor1";

    [Header("条件")]
    public bool RequiresKey = false;
    public string RequiredKeyID = "key_normal";

    [Header("提示")]
    public string PromptText = "按 E 进入下一层";
    public string LockedPrompt = "需要钥匙!";

    private bool _playerNearby = false;

    void Update()
    {
        if (_playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            TryTeleport();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerNearby = true;
            MessageUI.Instance?.ShowPrompt(PromptText);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerNearby = false;
            MessageUI.Instance?.HidePrompt();
        }
    }

    void TryTeleport()
    {
        // 检查是否需要钥匙
        if (RequiresKey)
        {
            bool hasKey = false;
            foreach (var item in GameManager.Instance.PlayerData.Inventory)
            {
                if (item.ItemID == RequiredKeyID)
                {
                    hasKey = true;
                    break;
                }
            }

            if (!hasKey)
            {
                MessageUI.Instance?.ShowMessage(LockedPrompt);
                return;
            }
        }

        // 执行传送
        MessageUI.Instance?.HidePrompt();
        GameManager.Instance.LoadFloor(TargetFloor);
    }
}

public enum TeleportDirection
{
    Up,     // 向上
    Down,   // 向下
    SameFloor // 同层传送
}
