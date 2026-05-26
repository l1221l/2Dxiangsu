using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 场景UI管理器 - 使用场景中已有的Canvas，不创建新的
/// </summary>
public class SceneUI : MonoBehaviour
{
    public static SceneUI Instance { get; private set; }

    [Header("Canvas（必须指定场景中的Canvas）")]
    public Canvas TargetCanvas;

    // 面板引用
    private GameObject _inventoryPanel;
    private bool _inventoryPanelCreated = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 已存在实例，销毁新的
            Destroy(gameObject);
            return;
        }

        // 查找场景中的Canvas
        if (TargetCanvas == null)
        {
            TargetCanvas = FindObjectOfType<Canvas>();
        }

        if (TargetCanvas == null)
        {
            Debug.LogError("SceneUI: 找不到 Canvas! 请在Inspector中指定Canvas。");
            return;
        }

        Debug.Log($"SceneUI: 使用Canvas: {TargetCanvas.name}");
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

    /// <summary>
    /// 切换背包显示
    /// </summary>
    void ToggleInventory()
    {
        if (!_inventoryPanelCreated || _inventoryPanel == null)
        {
            // 第一次按I，创建背包面板
            CreateInventoryPanel();
        }

        if (_inventoryPanel != null)
        {
            bool isActive = _inventoryPanel.activeSelf;
            _inventoryPanel.SetActive(!isActive);
            
            if (!isActive)
            {
                // 显示背包
                if (GameManager.Instance != null)
                    GameManager.Instance.SetGameState(GameState.Menu);
                
                // 刷新物品列表
                InventoryUI invUI = _inventoryPanel.GetComponent<InventoryUI>();
                if (invUI != null)
                    invUI.RefreshInventory();
            }
            else
            {
                // 隐藏背包
                if (GameManager.Instance != null)
                    GameManager.Instance.SetGameState(GameState.Exploration);
            }
        }
    }

    /// <summary>
    /// 创建背包面板
    /// </summary>
    void CreateInventoryPanel()
    {
        // 先检查是否已创建
        if (_inventoryPanel != null)
        {
            _inventoryPanelCreated = true;
            return;
        }

        if (TargetCanvas == null)
        {
            Debug.LogError("SceneUI: Canvas 为 null，无法创建背包面板");
            return;
        }

        // 先检查Canvas下是否已有背包面板
        Transform existingPanel = TargetCanvas.transform.Find("InventoryPanel") 
            ?? TargetCanvas.transform.Find("zhuangbeipanel")
            ?? TargetCanvas.transform.Find("item");

        if (existingPanel != null)
        {
            // 使用场景中已有的面板
            _inventoryPanel = existingPanel.gameObject;
            _inventoryPanel.name = "InventoryPanel";
            
            // 确保有InventoryUI组件
            InventoryUI invUI = _inventoryPanel.GetComponent<InventoryUI>();
            if (invUI == null) 
                invUI = _inventoryPanel.AddComponent<InventoryUI>();
            invUI.InventoryPanel = _inventoryPanel;

            _inventoryPanelCreated = true;
            Debug.Log("SceneUI: 使用场景中已有的背包面板");
            return;
        }

        // 没有现有面板，从Resources加载创建
        Debug.Log("SceneUI: 从Resources加载背包面板...");

        GameObject inventoryPrefab = Resources.Load<GameObject>("zhuangbeipanel") 
            ?? Resources.Load<GameObject>("item")
            ?? Resources.Load<GameObject>("InventoryPanel");

        if (inventoryPrefab != null)
        {
            _inventoryPanel = Instantiate(inventoryPrefab, TargetCanvas.transform);
            _inventoryPanel.name = "InventoryPanel";
            _inventoryPanel.SetActive(false);

            InventoryUI invUI = _inventoryPanel.GetComponent<InventoryUI>();
            if (invUI == null) 
                invUI = _inventoryPanel.AddComponent<InventoryUI>();
            invUI.InventoryPanel = _inventoryPanel;

            _inventoryPanelCreated = true;
            Debug.Log($"SceneUI: 背包面板创建完成: {inventoryPrefab.name}");
        }
        else
        {
            Debug.LogError("SceneUI: 找不到背包面板预制体！");
        }
    }

    /// <summary>
    /// 重置面板（场景切换时调用）
    /// </summary>
    public void ResetPanelsForNewScene()
    {
        // 销毁旧面板
        if (_inventoryPanel != null) 
        {
            Destroy(_inventoryPanel);
            _inventoryPanel = null;
        }

        _inventoryPanelCreated = false;

        // 重新查找Canvas
        if (TargetCanvas == null)
            TargetCanvas = FindObjectOfType<Canvas>();

        Debug.Log("SceneUI: 面板已重置用于新场景");
    }

    /// <summary>
    /// 游戏重新开始时重置
    /// </summary>
    public static void ResetPanels()
    {
        if (Instance != null)
        {
            Instance.ResetPanelsForNewScene();
        }
    }
}
