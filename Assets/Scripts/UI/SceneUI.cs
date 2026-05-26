using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 场景UI管理器 - 动态创建所有 UI 面板
/// 只需要场景中有一个 Canvas，所有面板都会动态生成作为子节点
/// </summary>
public class SceneUI : MonoBehaviour
{
    [Header("Canvas（可选，会自动查找）")]
    public Canvas TargetCanvas;

    // 面板引用
    private GameObject _battlePanel;
    private GameObject _messagePanel;
    private GameObject _promptPanel;
    private GameObject _inventoryPanel;

    // 防止重复创建
    private static bool _panelsCreated = false;

    void Awake()
    {
        if (_panelsCreated)
        {
            // 面板已创建，只需初始化组件引用
            InitExistingPanels();
            Destroy(gameObject); // 销毁多余的 SceneUI
            return;
        }

        if (TargetCanvas == null)
            TargetCanvas = FindObjectOfType<Canvas>();

        if (TargetCanvas == null)
        {
            Debug.LogError("SceneUI: 找不到 Canvas!");
            return;
        }

        CreateAllPanels();
        InitUI();
        _panelsCreated = true;
    }

    /// <summary>
    /// 初始化已存在的面板（从其他场景继承的）
    /// </summary>
    void InitExistingPanels()
    {
        if (TargetCanvas == null)
            TargetCanvas = FindObjectOfType<Canvas>();

        if (TargetCanvas == null) return;

        // 查找已创建的面板
        _battlePanel = TargetCanvas.transform.Find("BattlePanel")?.gameObject;
        _messagePanel = TargetCanvas.transform.Find("MessagePanel")?.gameObject;
        _promptPanel = TargetCanvas.transform.Find("PromptPanel")?.gameObject;
        _inventoryPanel = TargetCanvas.transform.Find("InventoryPanel")?.gameObject;

        // 添加/获取组件
        if (_battlePanel != null)
        {
            BattleUI battleUI = _battlePanel.GetComponent<BattleUI>();
            if (battleUI == null) battleUI = _battlePanel.AddComponent<BattleUI>();
            if (BattleManager.Instance != null) BattleManager.Instance.BattleUI = battleUI;
        }

        if (_inventoryPanel != null)
        {
            InventoryUI invUI = _inventoryPanel.GetComponent<InventoryUI>();
            if (invUI == null) invUI = _inventoryPanel.AddComponent<InventoryUI>();
        }

        if (MessageUI.Instance != null)
        {
            MessageUI.Instance.MessagePanel = _messagePanel;
            MessageUI.Instance.PromptPanel = _promptPanel;
            if (_messagePanel != null)
                MessageUI.Instance.MessageText = _messagePanel.transform.Find("MessageText")?.GetComponent<TextMeshProUGUI>();
            if (_promptPanel != null)
                MessageUI.Instance.PromptText = _promptPanel.transform.Find("PromptText")?.GetComponent<TextMeshProUGUI>();
        }
    }

    /// <summary>
    /// 动态创建所有 UI 面板
    /// </summary>
    void CreateAllPanels()
    {
        Transform canvasTransform = TargetCanvas.transform;

        // --- 创建战斗面板 ---
        _battlePanel = CreatePanel("BattlePanel", canvasTransform);
        _battlePanel.SetActive(false);
        
        // 玩家信息区域
        GameObject playerInfo = CreateSubPanel("PlayerInfo", _battlePanel.transform);
        CreateText("NameText", "玩家", playerInfo.transform, new Vector2(0, 50));
        CreateSlider("HPSlider", playerInfo.transform, new Vector2(0, 20));
        CreateText("HPText", "100/100", playerInfo.transform, new Vector2(0, -10));

        // 敌人信息区域
        GameObject enemyInfo = CreateSubPanel("EnemyInfo", _battlePanel.transform);
        CreateText("NameText", "敌人", enemyInfo.transform, new Vector2(0, 50));
        CreateSlider("HPSlider", enemyInfo.transform, new Vector2(0, 20));
        CreateText("HPText", "50/50", enemyInfo.transform, new Vector2(0, -10));

        // 消息文本
        CreateText("MessageText", "战斗开始！", _battlePanel.transform, new Vector2(0, -50));

        // 操作菜单
        GameObject actionMenu = CreateSubPanel("ActionMenu", _battlePanel.transform);
        CreateText("PromptText", "[1] 攻击  [2] 物品  [3] 逃跑", actionMenu.transform, new Vector2(0, 0));

        // --- 创建消息面板 ---
        _messagePanel = CreatePanel("MessagePanel", canvasTransform);
        _messagePanel.SetActive(false);
        CreateText("MessageText", "", _messagePanel.transform, new Vector2(0, 0));

        // --- 创建提示面板 ---
        _promptPanel = CreatePanel("PromptPanel", canvasTransform);
        _promptPanel.SetActive(false);
        CreateText("PromptText", "", _promptPanel.transform, new Vector2(0, 0));

        // --- 创建背包面板 ---
        _inventoryPanel = CreatePanel("InventoryPanel", canvasTransform);
        _inventoryPanel.SetActive(false);
        
        // 滚动视图
        GameObject scrollView = CreateSubPanel("Scroll View", _inventoryPanel.transform);
        GameObject viewport = CreateSubPanel("Viewport", scrollView.transform);
        CreateSubPanel("Content", viewport.transform);
        
        // 物品详情
        GameObject itemDetail = CreateSubPanel("ItemDetail", _inventoryPanel.transform);
        CreateText("name", "物品名称", itemDetail.transform, new Vector2(0, 50));
        CreateText("jianjie", "物品描述", itemDetail.transform, new Vector2(0, 0));
        CreateImage("itemImage", itemDetail.transform, new Vector2(0, -50));
        CreateButton("Button (Legacy)", "使用", itemDetail.transform, new Vector2(100, -100));
        
        // 关闭按钮
        CreateButton("returnbtn", "关闭", _inventoryPanel.transform, new Vector2(200, 200));

        Debug.Log("SceneUI: 所有面板动态创建完成");
    }

    /// <summary>
    /// 创建基础面板
    /// </summary>
    GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0.8f);
        
        return panel;
    }

    /// <summary>
    /// 创建子面板
    /// </summary>
    GameObject CreateSubPanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        panel.AddComponent<RectTransform>();
        return panel;
    }

    /// <summary>
    /// 创建文本
    /// </summary>
    TextMeshProUGUI CreateText(string name, string text, Transform parent, Vector2 position)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(300, 50);
        
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        return tmp;
    }

    /// <summary>
    /// 创建滑块
    /// </summary>
    Slider CreateSlider(string name, Transform parent, Vector2 position)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(200, 20);
        
        Slider slider = go.AddComponent<Slider>();
        
        // 创建背景
        GameObject background = new GameObject("Background");
        background.transform.SetParent(go.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = Color.gray;
        
        // 创建填充区域
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = Color.green;
        
        slider.fillRect = fill.GetComponent<RectTransform>();
        
        return slider;
    }

    /// <summary>
    /// 创建图片
    /// </summary>
    Image CreateImage(string name, Transform parent, Vector2 position)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(64, 64);
        
        Image image = go.AddComponent<Image>();
        image.color = Color.white;
        
        return image;
    }

    /// <summary>
    /// 创建按钮
    /// </summary>
    Button CreateButton(string name, string text, Transform parent, Vector2 position)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(100, 40);
        
        Image image = go.AddComponent<Image>();
        image.color = new Color(0.2f, 0.6f, 1f);
        
        Button button = go.AddComponent<Button>();
        
        // 创建按钮文本
        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        RectTransform textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return button;
    }

    /// <summary>
    /// 初始化 UI 组件
    /// </summary>
    void InitUI()
    {
        // --- BattleUI ---
        BattleUI battleUI = _battlePanel.AddComponent<BattleUI>();
        battleUI.BattlePanel = _battlePanel;
        
        if (BattleManager.Instance != null)
            BattleManager.Instance.BattleUI = battleUI;

        // --- InventoryUI ---
        InventoryUI invUI = _inventoryPanel.AddComponent<InventoryUI>();
        invUI.InventoryPanel = _inventoryPanel;

        // --- MessageUI ---
        if (MessageUI.Instance != null)
        {
            MessageUI.Instance.MessagePanel = _messagePanel;
            MessageUI.Instance.PromptPanel = _promptPanel;
            MessageUI.Instance.MessageText = _messagePanel.transform.Find("MessageText")?.GetComponent<TextMeshProUGUI>();
            MessageUI.Instance.PromptText = _promptPanel.transform.Find("PromptText")?.GetComponent<TextMeshProUGUI>();
        }

        Debug.Log($"SceneUI: UI 初始化完成");
    }

    /// <summary>
    /// 重置面板创建状态（游戏重新开始时调用）
    /// </summary>
    public static void ResetPanels()
    {
        _panelsCreated = false;
    }
}
