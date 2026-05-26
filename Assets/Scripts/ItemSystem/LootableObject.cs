using UnityEngine;

/// <summary>
/// 可拾取物品对象（箱子、木架等）
/// </summary>
public class LootableObject : MonoBehaviour
{
    [Header("物品设置")]
    public string[] PossibleItemIDs; // 可能掉落的物品ID列表
    public int MinItems = 1;
    public int MaxItems = 3;
    public bool DestroyAfterLoot = true;

    [Header("外观")]
    public Sprite OpenedSprite; // 打开后的精灵图
    public GameObject GlowEffect; // 发光效果

    [Header("提示文本")]
    public string InteractionPrompt = "按 E 拾取";

    private bool _isOpened = false;
    private SpriteRenderer _spriteRenderer;
    private bool _playerNearby = false;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (GlowEffect != null)
            GlowEffect.SetActive(false);
        
        Debug.Log($"[{gameObject.name}] LootableObject 初始化完成");
    }

    void Update()
    {
        if (_playerNearby && !_isOpened && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"[{gameObject.name}] 按E拾取！");
            OpenLoot();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[{gameObject.name}] 触发器进入: {other.name}, Tag: {other.tag}");
        
        if (other.CompareTag("Character"))
        {
            _playerNearby = true;
            if (GlowEffect != null)
                GlowEffect.SetActive(true);
            
            // 显示提示
            if (MessageUI.Instance != null)
            {
                MessageUI.Instance.ShowPrompt(InteractionPrompt);
                Debug.Log($"[{gameObject.name}] 显示提示: {InteractionPrompt}");
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] MessageUI.Instance 为 null!");
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"[{gameObject.name}] 触发器离开: {other.name}");
        
        if (other.CompareTag("Character"))
        {
            _playerNearby = false;
            if (GlowEffect != null)
                GlowEffect.SetActive(false);
            
            MessageUI.Instance?.HidePrompt();
        }
    }

    void OpenLoot()
    {
        if (_isOpened) return;
        _isOpened = true;

        Debug.Log($"[{gameObject.name}] 打开宝箱！");

        // 生成随机物品
        int itemCount = Random.Range(MinItems, MaxItems + 1);
        string lootMessage = "获得物品:\n";

        for (int i = 0; i < itemCount; i++)
        {
            if (PossibleItemIDs.Length > 0)
            {
                string randomItemID = PossibleItemIDs[Random.Range(0, PossibleItemIDs.Length)];
                Item item = ItemDatabase.GetItem(randomItemID);
                if (item != null)
                {
                    GameManager.Instance.PlayerData.AddItem(item);
                    lootMessage += $"- {item.ItemName}\n";
                    Debug.Log($"[{gameObject.name}] 获得物品: {item.ItemName}");
                }
            }
        }

        // 显示获得物品提示
        MessageUI.Instance?.ShowMessage(lootMessage, 3f);

        // 改变外观
        if (OpenedSprite != null && _spriteRenderer != null)
        {
            _spriteRenderer.sprite = OpenedSprite;
        }

        // 隐藏提示
        MessageUI.Instance?.HidePrompt();

        // 是否销毁
        if (DestroyAfterLoot)
        {
            // 延迟销毁，让玩家看到变化
            Invoke(nameof(DestroyObject), 0.5f);
        }
    }

    void DestroyObject()
    {
        Destroy(gameObject);
    }
}
