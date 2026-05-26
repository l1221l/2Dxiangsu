using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 背包UI - 面板由 SceneUI 动态创建
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("背包面板")]
    public GameObject InventoryPanel;
    
    [Header("物品槽预设（需要手动设置或在SceneUI中设置）")]
    public GameObject ItemSlotPrefab;

    // 内部引用
    private Transform _itemContainer;
    private GameObject _itemDetailPanel;
    private Text _itemNameText;
    private Text _itemDescText;
    private Image _itemIcon;
    private Button _useButton;
    private Button _closeButton;

    private List<GameObject> _slotObjects = new List<GameObject>();
    private Item _selectedItem;

    void Awake()
    {
        if (InventoryPanel == null)
            InventoryPanel = gameObject;

        AutoBind();
    }

    void Start()
    {
        if (InventoryPanel != null)
            InventoryPanel.SetActive(false);
        if (_itemDetailPanel != null)
            _itemDetailPanel.SetActive(false);
    }

    /// <summary>
    /// 自动查找子节点绑定引用
    /// </summary>
    void AutoBind()
    {
        Transform root = InventoryPanel.transform;

        // 物品容器
        _itemContainer = root.Find("Scroll View/Viewport/Content");

        // 物品详情面板
        _itemDetailPanel = root.Find("ItemDetail")?.gameObject;

        if (_itemDetailPanel != null)
        {
            _itemNameText = _itemDetailPanel.transform.Find("name")?.GetComponent<Text>();
            _itemDescText = _itemDetailPanel.transform.Find("jianjie")?.GetComponent<Text>();
            _itemIcon = _itemDetailPanel.transform.Find("itemImage")?.GetComponent<Image>();
            
            Button btn = _itemDetailPanel.transform.Find("Button (Legacy)")?.GetComponent<Button>();
            if (btn != null)
            {
                // 移除所有现有监听器，防止重复
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(UseSelectedItem);
                _useButton = btn;
            }
        }

        // 关闭按钮
        Button closeBtn = root.Find("returnbtn")?.GetComponent<Button>();
        if (closeBtn != null)
        {
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener(CloseInventory);
            _closeButton = closeBtn;
        }
    }

    void Update()
    {
        // 按 I 打开/关闭背包
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Battle)
            {
                ToggleInventory();
            }
        }
    }

    public void ToggleInventory()
    {
        if (InventoryPanel.activeSelf)
            CloseInventory();
        else
            OpenInventory();
    }

    public void OpenInventory()
    {
        if (InventoryPanel == null) return;
        
        InventoryPanel.SetActive(true);
        if (GameManager.Instance != null)
            GameManager.Instance.SetGameState(GameState.Menu);
        RefreshInventory();
    }

    public void CloseInventory()
    {
        if (InventoryPanel == null) return;
        
        InventoryPanel.SetActive(false);
        if (_itemDetailPanel != null)
            _itemDetailPanel.SetActive(false);
        if (GameManager.Instance != null)
            GameManager.Instance.SetGameState(GameState.Exploration);
    }

    void RefreshInventory()
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
        for (int i = 0; i < inventory.Count; i++)
        {
            Item item = inventory[i];
            
            // 如果没有预设，使用简单的文本创建
            if (ItemSlotPrefab == null || _itemContainer == null)
            {
                CreateSimpleSlot(item, i);
            }
            else
            {
                CreatePrefabricatedSlot(item, i);
            }
        }
    }

    /// <summary>
    /// 创建简单物品槽（无预设时使用）
    /// </summary>
    void CreateSimpleSlot(Item item, int index)
    {
        if (_itemContainer == null) return;

        GameObject slot = new GameObject("Slot_" + index);
        slot.transform.SetParent(_itemContainer, false);
        
        RectTransform rect = slot.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100, 50);
        
        Image bg = slot.AddComponent<Image>();
        bg.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);

        TextMeshProUGUI text = slot.AddComponent<TextMeshProUGUI>();
        text.text = item.ItemName;
        text.fontSize = 16;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        Button btn = slot.AddComponent<Button>();
        btn.onClick.AddListener(() => SelectItem(index));

        _slotObjects.Add(slot);
    }

    /// <summary>
    /// 使用预设创建物品槽
    /// </summary>
    void CreatePrefabricatedSlot(Item item, int index)
    {
        GameObject slot = Instantiate(ItemSlotPrefab, _itemContainer);
        _slotObjects.Add(slot);

        TextMeshProUGUI nameText = slot.GetComponentInChildren<TextMeshProUGUI>();
        if (nameText != null)
            nameText.text = item.ItemName;

        Button button = slot.GetComponent<Button>();
        if (button != null)
        {
            // 移除现有监听器
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SelectItem(index));
        }
    }

    void SelectItem(int index)
    {
        if (GameManager.Instance == null) return;
        
        var inventory = GameManager.Instance.PlayerData.Inventory;
        if (index >= 0 && index < inventory.Count)
        {
            _selectedItem = inventory[index];
            ShowItemDetail(_selectedItem);
        }
    }

    void ShowItemDetail(Item item)
    {
        if (_itemDetailPanel == null) return;
        
        _itemDetailPanel.SetActive(true);

        if (_itemNameText != null) _itemNameText.text = item.ItemName;
        if (_itemDescText != null) _itemDescText.text = item.Description;
        if (item.Icon != null && _itemIcon != null) _itemIcon.sprite = item.Icon;

        if (_useButton != null)
            _useButton.interactable = (item.Type == ItemType.Consumable);
    }

    void UseSelectedItem()
    {
        if (_selectedItem == null) return;
        if (GameManager.Instance == null) return;

        if (_selectedItem.Type == ItemType.Consumable)
        {
            int healAmount = _selectedItem.Value;
            GameManager.Instance.PlayerData.Heal(healAmount);
            GameManager.Instance.PlayerData.RemoveItem(_selectedItem);

            MessageUI.Instance?.ShowMessage($"使用了 {_selectedItem.ItemName}, 恢复 {healAmount} 点生命值!");

            RefreshInventory();
            if (_itemDetailPanel != null) _itemDetailPanel.SetActive(false);
            _selectedItem = null;
        }
    }
}
