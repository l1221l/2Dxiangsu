using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 背包UI - 适配 zhuangbeipanel 预制体结构
/// 点击道具显示 itempanel 详情面板
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("背包面板")]
    public GameObject InventoryPanel;
    public GameObject ItemSlotPrefab;

    // 内部引用
    private Transform _itemContainer;
    private Button _closeButton;

    // 详情面板
    private GameObject _itemDetailPanel;
    private Text _itemNameText;
    private Text _itemDescText;
    private Image _itemIcon;
    private Button _useButton;
    private Button _detailCloseButton;

    private List<GameObject> _slotObjects = new List<GameObject>();
    private Item _selectedItem;
    private int _selectedItemIndex = -1;

    void Awake()
    {
        if (InventoryPanel == null)
            InventoryPanel = gameObject;

        AutoBind();
        CreateItemDetailPanel();
    }

    void Start()
    {
        // 初始隐藏
        if (InventoryPanel != null)
            InventoryPanel.SetActive(false);
    }

    /// <summary>
    /// 自动查找子节点绑定引用
    /// </summary>
    void AutoBind()
    {
        Transform root = InventoryPanel.transform;

        // 物品容器 - zhuangbeipanel 结构
        _itemContainer = root.Find("Scroll View/Viewport/Content");
        if (_itemContainer == null)
        {
            _itemContainer = root.Find("Content");
        }

        // 关闭按钮
        Transform closeBtnTransform = root.Find("returnbtn") 
            ?? root.Find("Scroll View/returnbtn")
            ?? root.Find("returnbtn/Text (Legacy)");
            
        if (closeBtnTransform != null)
        {
            if (closeBtnTransform.GetComponent<Button>() == null)
            {
                closeBtnTransform = closeBtnTransform.parent;
            }
            
            _closeButton = closeBtnTransform.GetComponent<Button>();
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveAllListeners();
                _closeButton.onClick.AddListener(CloseInventory);
            }
        }

        Debug.Log($"InventoryUI: 绑定完成, _itemContainer={_itemContainer != null}");
    }

    /// <summary>
    /// 创建/查找道具详情面板
    /// </summary>
    void CreateItemDetailPanel()
    {
        if (InventoryPanel == null)
        {
            Debug.LogError("InventoryUI: InventoryPanel 为 null，无法创建详情面板");
            return;
        }
        
        // 尝试在背包面板下查找已有的 itempanel
        _itemDetailPanel = InventoryPanel.transform.Find("itempanel")?.gameObject;

        // 如果没有，从 Resources 加载（不设置父节点，避免持久化问题）
        if (_itemDetailPanel == null)
        {
            GameObject detailPrefab = Resources.Load<GameObject>("itempanel");
            if (detailPrefab != null)
            {
                _itemDetailPanel = Instantiate(detailPrefab);
                _itemDetailPanel.name = "itempanel";
                DontDestroyOnLoad(_itemDetailPanel);
            }
        }

        if (_itemDetailPanel != null)
        {
            _itemDetailPanel.SetActive(false);

            // 查找子组件
            Transform root = _itemDetailPanel.transform;
            _itemNameText = root.Find("name")?.GetComponent<Text>();
            _itemDescText = root.Find("jianjie")?.GetComponent<Text>();
            _itemIcon = root.Find("itemImage")?.GetComponent<Image>();

            // 使用按钮
            Transform useBtnTransform = root.Find("Button (Legacy)") ?? root.Find("UseButton");
            if (useBtnTransform != null)
            {
                _useButton = useBtnTransform.GetComponent<Button>();
                if (_useButton != null)
                {
                    _useButton.onClick.RemoveAllListeners();
                    _useButton.onClick.AddListener(OnUseButtonClick);
                }
            }

            // 关闭按钮
            Transform closeBtnTransform = root.Find("returnbtn") ?? root.Find("CloseButton");
            if (closeBtnTransform != null)
            {
                _detailCloseButton = closeBtnTransform.GetComponent<Button>();
                if (_detailCloseButton != null)
                {
                    _detailCloseButton.onClick.RemoveAllListeners();
                    _detailCloseButton.onClick.AddListener(HideItemDetail);
                }
            }

            Debug.Log($"InventoryUI: 详情面板绑定完成");
        }
        else
        {
            Debug.LogWarning("InventoryUI: 找不到 itempanel 预制体");
        }
    }

    public void ToggleInventory()
    {
        if (InventoryPanel == null)
        {
            Debug.LogError("InventoryUI: InventoryPanel 为 null!");
            return;
        }

        if (InventoryPanel.activeSelf)
            CloseInventory();
        else
            OpenInventory();
    }

    public void OpenInventory()
    {
        if (InventoryPanel == null)
        {
            Debug.LogError("InventoryUI: 无法打开背包，InventoryPanel 为 null!");
            return;
        }

        Debug.Log("InventoryUI: 打开背包");
        InventoryPanel.SetActive(true);
        
        if (GameManager.Instance != null)
            GameManager.Instance.SetGameState(GameState.Menu);
        
        RefreshInventory();
    }

    public void CloseInventory()
    {
        if (InventoryPanel == null) return;
        
        // 先关闭详情面板
        HideItemDetail();
        
        InventoryPanel.SetActive(false);
        
        if (GameManager.Instance != null)
            GameManager.Instance.SetGameState(GameState.Exploration);
    }

    public void RefreshInventory()
    {
        // 清除旧槽位
        foreach (var slot in _slotObjects)
        {
            if (slot != null)
                Destroy(slot);
        }
        _slotObjects.Clear();

        if (GameManager.Instance == null) return;

        var inventory = GameManager.Instance.PlayerData.Inventory;
        Debug.Log($"InventoryUI: 刷新背包，物品数量: {inventory.Count}");

        for (int i = 0; i < inventory.Count; i++)
        {
            Item item = inventory[i];
            CreateItemSlot(item, i);
        }
    }

    /// <summary>
    /// 创建物品槽 - 使用预制体，只绑定点击事件
    /// </summary>
    void CreateItemSlot(Item item, int index)
    {
        if (_itemContainer == null)
        {
            Debug.LogWarning("InventoryUI: _itemContainer 为 null，无法创建物品槽");
            return;
        }

        GameObject slot;
        
        if (ItemSlotPrefab != null)
        {
            // 使用预设创建
            slot = Instantiate(ItemSlotPrefab, _itemContainer);
            slot.name = "Slot_" + index;
            
            // 获取预制体自带的 Button 组件，绑定点击事件
            Button button = slot.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                int itemIndex = index;
                button.onClick.AddListener(() => OnItemClick(itemIndex));
                Debug.Log($"InventoryUI: 物品槽 {index} 按钮绑定完成");
            }
            else
            {
                Debug.LogWarning($"InventoryUI: 物品槽 {index} 没有找到 Button 组件！");
            }
        }
        else
        {
            // 没有预设时的备用方案
            slot = new GameObject("Slot_" + index);
            slot.transform.SetParent(_itemContainer, false);
            
            RectTransform rect = slot.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(80, 80);
            
            Image bg = slot.AddComponent<Image>();
            bg.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

            Button btn = slot.AddComponent<Button>();
            btn.targetGraphic = bg;
            int itemIndex = index;
            btn.onClick.AddListener(() => OnItemClick(itemIndex));
        }

        _slotObjects.Add(slot);
    }

    /// <summary>
    /// 点击物品槽 - 显示详情面板
    /// </summary>
    void OnItemClick(int index)
    {
        Debug.Log($"InventoryUI: 点击物品槽 {index}");
        
        if (GameManager.Instance == null) 
        {
            Debug.LogWarning("InventoryUI: GameManager 为 null");
            return;
        }
        
        var inventory = GameManager.Instance.PlayerData.Inventory;
        if (index >= 0 && index < inventory.Count)
        {
            _selectedItem = inventory[index];
            _selectedItemIndex = index;
            
            Debug.Log($"InventoryUI: 点击物品: {_selectedItem.ItemName}");
            ShowItemDetail(_selectedItem);
        }
        else
        {
            Debug.LogWarning($"InventoryUI: 物品索引 {index} 超出范围，背包物品数: {inventory.Count}");
        }
    }

    /// <summary>
    /// 显示道具详情
    /// </summary>
    void ShowItemDetail(Item item)
    {
        if (_itemDetailPanel == null) return;

        // 显示时把详情面板放到背包下（临时）
        if (InventoryPanel != null)
        {
            _itemDetailPanel.transform.SetParent(InventoryPanel.transform, false);
        }

        // 设置信息
        if (_itemNameText != null) _itemNameText.text = item.ItemName;
        if (_itemDescText != null) _itemDescText.text = item.Description;
        if (item.Icon != null && _itemIcon != null) _itemIcon.sprite = item.Icon;

        // 只有消耗品显示使用按钮
        if (_useButton != null)
        {
            _useButton.gameObject.SetActive(item.Type == ItemType.Consumable);
        }

        _itemDetailPanel.SetActive(true);
    }

    /// <summary>
    /// 隐藏详情面板
    /// </summary>
    void HideItemDetail()
    {
        if (_itemDetailPanel != null)
        {
            _itemDetailPanel.SetActive(false);
            // 移出背包面板，避免被销毁
            _itemDetailPanel.transform.SetParent(null);
        }
        
        _selectedItem = null;
        _selectedItemIndex = -1;
    }

    /// <summary>
    /// 点击使用按钮
    /// </summary>
    void OnUseButtonClick()
    {
        if (_selectedItem == null) return;
        if (GameManager.Instance == null) return;

        if (_selectedItem.Type == ItemType.Consumable)
        {
            int healAmount = _selectedItem.Value;
            GameManager.Instance.PlayerData.Heal(healAmount);
            GameManager.Instance.PlayerData.RemoveItem(_selectedItem);

            MessageUI.Instance?.ShowMessage($"使用了 {_selectedItem.ItemName}, 恢复 {healAmount} 点生命值!");

            HideItemDetail();
            RefreshInventory();
        }
    }
}
