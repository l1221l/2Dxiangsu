using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 战斗UI管理
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

    void Start()
    {
        HideBattleUI();
    }

    /// <summary>
    /// 显示战斗UI
    /// </summary>
    public void ShowBattleUI()
    {
        BattlePanel.SetActive(true);
        ActionPromptText.text = "[1] 攻击  [2] 物品  [3] 逃跑";
    }

    /// <summary>
    /// 隐藏战斗UI
    /// </summary>
    public void HideBattleUI()
    {
        BattlePanel.SetActive(false);
        ShowActionMenu(false);
    }

    /// <summary>
    /// 更新UI显示
    /// </summary>
    public void UpdateUI(BattleUnit player, BattleUnit enemy)
    {
        // 玩家信息
        PlayerNameText.text = player.Name;
        PlayerHPSlider.maxValue = player.MaxHP;
        PlayerHPSlider.value = player.CurrentHP;
        PlayerHPText.text = $"{player.CurrentHP}/{player.MaxHP}";
        
        // 敌人信息
        EnemyNameText.text = enemy.Name;
        EnemyHPSlider.maxValue = enemy.MaxHP;
        EnemyHPSlider.value = enemy.CurrentHP;
        EnemyHPText.text = $"{enemy.CurrentHP}/{enemy.MaxHP}";
    }

    /// <summary>
    /// 显示消息
    /// </summary>
    public void ShowMessage(string message)
    {
        MessageText.text = message;
    }

    /// <summary>
    /// 显示/隐藏操作菜单
    /// </summary>
    public void ShowActionMenu(bool show)
    {
        ActionMenu.SetActive(show);
    }
}
