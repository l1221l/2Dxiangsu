using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 战斗UI管理 - 挂载在 Canvas 下的 BattlePanel 上
/// 所有子节点通过 transform.Find 自动查找
/// </summary>
public class BattleUI : MonoBehaviour
{
    [Header("主面板")]
    public GameObject BattlePanel;
    
    [Header("玩家信息")]
    public TextMeshProUGUI PlayerNameText;
    public Slider PlayerHPSlider;
    public TextMeshProUGUI PlayerHPText;
    
    [Header("敌人信息")]
    public TextMeshProUGUI EnemyNameText;
    public Slider EnemyHPSlider;
    public TextMeshProUGUI EnemyHPText;
    
    [Header("消息显示")]
    public TextMeshProUGUI MessageText;
    
    [Header("操作菜单")]
    public GameObject ActionMenu;
    public TextMeshProUGUI ActionPromptText;

    void Awake()
    {
        if (BattlePanel == null)
            BattlePanel = gameObject;

        AutoBind();
        HideBattleUI();
    }

    /// <summary>
    /// 自动查找子节点绑定引用
    /// </summary>
    void AutoBind()
    {
        Transform root = BattlePanel.transform;

        // 玩家信息
        Transform playerInfo = root.Find("PlayerInfo");
        if (playerInfo != null)
        {
            if (PlayerNameText == null) PlayerNameText = playerInfo.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (PlayerHPSlider == null) PlayerHPSlider = playerInfo.Find("HPSlider")?.GetComponent<Slider>();
            if (PlayerHPText == null) PlayerHPText = playerInfo.Find("HPText")?.GetComponent<TextMeshProUGUI>();
        }

        // 敌人信息
        Transform enemyInfo = root.Find("EnemyInfo");
        if (enemyInfo != null)
        {
            if (EnemyNameText == null) EnemyNameText = enemyInfo.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (EnemyHPSlider == null) EnemyHPSlider = enemyInfo.Find("HPSlider")?.GetComponent<Slider>();
            if (EnemyHPText == null) EnemyHPText = enemyInfo.Find("HPText")?.GetComponent<TextMeshProUGUI>();
        }

        // 消息
        if (MessageText == null)
            MessageText = root.Find("MessageText")?.GetComponent<TextMeshProUGUI>();

        // 操作菜单
        if (ActionMenu == null)
        {
            Transform actionMenu = root.Find("ActionMenu");
            if (actionMenu != null)
                ActionMenu = actionMenu.gameObject;
        }
        if (ActionPromptText == null && ActionMenu != null)
            ActionPromptText = ActionMenu.transform.Find("PromptText")?.GetComponent<TextMeshProUGUI>();
    }

    public void ShowBattleUI()
    {
        BattlePanel.SetActive(true);
        if (ActionPromptText != null)
            ActionPromptText.text = "[1] 攻击  [2] 物品  [3] 逃跑";
    }

    public void HideBattleUI()
    {
        BattlePanel.SetActive(false);
        ShowActionMenu(false);
    }

    public void UpdateUI(BattleUnit player, BattleUnit enemy)
    {
        if (PlayerNameText != null) PlayerNameText.text = player.Name;
        if (PlayerHPSlider != null) { PlayerHPSlider.maxValue = player.MaxHP; PlayerHPSlider.value = player.CurrentHP; }
        if (PlayerHPText != null) PlayerHPText.text = $"{player.CurrentHP}/{player.MaxHP}";
        
        if (EnemyNameText != null) EnemyNameText.text = enemy.Name;
        if (EnemyHPSlider != null) { EnemyHPSlider.maxValue = enemy.MaxHP; EnemyHPSlider.value = enemy.CurrentHP; }
        if (EnemyHPText != null) EnemyHPText.text = $"{enemy.CurrentHP}/{enemy.MaxHP}";
    }

    public void ShowMessage(string message)
    {
        if (MessageText != null) MessageText.text = message;
    }

    public void ShowActionMenu(bool show)
    {
        if (ActionMenu != null) ActionMenu.SetActive(show);
    }
}
