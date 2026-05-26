using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 背包UI
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("背包面板")]
    public GameObject InventoryPanel;
    private Transform ItemContainer;
    public GameObject ItemSlotPrefab;

    [Header("物品详情")]
    public GameObject ItemDetailPanel;
    private Text ItemNameText;
    private Text ItemDescText;
    private Image ItemIcon;

    [Header("按钮")]
    public Button UseButton;
    public Button CloseButton;

    private List<GameObject> _slotObjects = new List<GameObject>();
    private Item _selectedItem;

    void Start()
    {
        InventoryPanel.SetActive(false);
        ItemDetailPanel.SetActive(false);

        CloseButton?.onClick.AddListener(CloseInventory);
        UseButton?.onClick.AddListener(UseSelectedItem);
    }

    void Update()
    {
        ItemContainer= InventoryPanel.transform.Find("Scroll View/Viewport/Content");
        ItemNameText = ItemDetailPanel.transform.Find("name").GetComponent<Text>();
        ItemDescText = ItemDetailPanel.transform.Find("jianjie").GetComponent<Text>();
        ItemIcon = ItemDetailPanel.transform.Find("itemImage").GetComponent<Image>();
        UseButton = ItemDetailPanel.transform.Find("Button (Legacy)").GetComponent<Button>();
        CloseButton= InventoryPanel.transform.Find("returnbtn").GetComponent<Button>();
        // 按 I 打开/关闭背包
        if (Input.GetKeyDown(KeyCode.I) && GameManager.Instance.CurrentState != GameState.Battle)
        {
            ToggleInventory();
        }
    }

    /// <summary>
    /// 切换背包显示
    /// </summary>
    public void ToggleInventory()
    {
        if (InventoryPanel.activeSelf)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }

    /// <summary>
    /// 打开背包
    /// </summary>
    public void OpenInventory()
    {
        InventoryPanel.SetActive(true);
        GameManager.Instance.SetGameState(GameState.Menu);
        RefreshInventory();
    }

    /// <summary>
    /// 关闭背包
    /// </summary>
    public void CloseInventory()
    {
        InventoryPanel.SetActive(false);
        ItemDetailPanel.SetActive(false);
        GameManager.Instance.SetGameState(GameState.Exploration);
    }

    /// <summary>
    /// 刷新背包显示
    /// </summary>
    void RefreshInventory()
    {
        // 清除旧槽位
        foreach (var slot in _slotObjects)
        {
            Destroy(slot);
        }
        _slotObjects.Clear();

        // 创建新槽位
        var inventory = GameManager.Instance.PlayerData.Inventory;
        for (int i = 0; i < inventory.Count; i++)
        {
            Item item = inventory[i];
            GameObject slot = Instantiate(ItemSlotPrefab, ItemContainer);
            _slotObjects.Add(slot);

            // 设置槽位信息
            TextMeshProUGUI nameText = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = item.ItemName;

            // 添加点击事件
            Button button = slot.GetComponent<Button>();
            if (button != null)
            {
                int index = i; // 捕获索引
                button.onClick.AddListener(() => SelectItem(index));
            }
        }
    }

    /// <summary>
    /// 选择物品
    /// </summary>
    void SelectItem(int index)
    {
        var inventory = GameManager.Instance.PlayerData.Inventory;
        if (index >= 0 && index < inventory.Count)
        {
            _selectedItem = inventory[index];
            ShowItemDetail(_selectedItem);
        }
    }

    /// <summary>
    /// 显示物品详情
    /// </summary>
    void ShowItemDetail(Item item)
    {
        ItemDetailPanel.SetActive(true);
        ItemNameText.text = item.ItemName;
        ItemDescText.text = item.Description;
        
        if (item.Icon != null)
            ItemIcon.sprite = item.Icon;

        // 只有消耗品可以使用
        UseButton.interactable = (item.Type == ItemType.Consumable);
    }

    /// <summary>
    /// 使用选中的物品
    /// </summary>
    void UseSelectedItem()
    {
        if (_selectedItem == null) return;

        if (_selectedItem.Type == ItemType.Consumable)
        {
            // 恢复生命值
            int healAmount = _selectedItem.Value;
            GameManager.Instance.PlayerData.Heal(healAmount);
            GameManager.Instance.PlayerData.RemoveItem(_selectedItem);

            MessageUI.Instance?.ShowMessage($"使用了 {_selectedItem.ItemName}, 恢复 {healAmount} 点生命值!");

            // 刷新显示
            RefreshInventory();
            ItemDetailPanel.SetActive(false);
            _selectedItem = null;
        }
    }
}
