using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 战斗指令菜单控制器 - 连接UI按钮与战斗逻辑
/// </summary>
public class BattleEmenuCtrl : MonoBehaviour
{
    [Header("主菜单按钮")]
    [SerializeField] private Button attackMenu;
    [SerializeField] private Button defendMenu;
    [SerializeField] private Button itemMenu;
    [SerializeField] private Button runMenu;

    [Header("攻击菜单")]
    [SerializeField] private GameObject attackPanel;
    [SerializeField] private Button skill1Button;
    [SerializeField] private Button skill2Button;
    [SerializeField] private Button skill3Button;
    [SerializeField] private Button skill4Button;

    [Header("技能按钮文本")]
    [SerializeField] private Text skill1Text;
    [SerializeField] private Text skill2Text;
    [SerializeField] private Text skill3Text;
    [SerializeField] private Text skill4Text;

    [Header("UI状态")]
    [SerializeField] private Text turnIndicator;
    [SerializeField] private GameObject playerTurnPanel;
    [SerializeField] private GameObject enemyTurnPanel;

    private BattleManager battleManager;

    void Start()
    {
        battleManager = BattleManager.Instance;

        // 绑定主菜单按钮事件
        attackMenu.onClick.AddListener(OnAttackMenu);
        defendMenu.onClick.AddListener(OnDefendMenu);
        itemMenu.onClick.AddListener(OnItemMenu);
        runMenu.onClick.AddListener(OnRunMenu);

        // 绑定技能按钮事件
        skill1Button.onClick.AddListener(() => OnSkillButton(0));
        skill2Button.onClick.AddListener(() => OnSkillButton(1));
        skill3Button.onClick.AddListener(() => OnSkillButton(2));
        skill4Button.onClick.AddListener(OnCloseAttackMenu);

        // 初始化技能按钮文本
        UpdateSkillButtons();
    }

    void Update()
    {
        UpdateTurnIndicator();
    }

    /// <summary>
    /// 更新技能按钮文本
    /// </summary>
    void UpdateSkillButtons()
    {
        if (battleManager == null) return;

        List<SkillData> skills = battleManager.GetPlayerSkills();
        
        if (skills.Count > 0 && skill1Text != null)
            skill1Text.text = $"{skills[0].skillName}\nMP: {skills[0].mpCost}";
        
        if (skills.Count > 1 && skill2Text != null)
            skill2Text.text = $"{skills[1].skillName}\nMP: {skills[1].mpCost}";
        
        if (skills.Count > 2 && skill3Text != null)
            skill3Text.text = $"{skills[2].skillName}\nMP: {skills[2].mpCost}";
        
        if (skill4Text != null)
            skill4Text.text = "返回";
    }

    /// <summary>
    /// 更新回合指示器
    /// </summary>
    void UpdateTurnIndicator()
    {
        if (battleManager == null) return;

        if (turnIndicator != null)
        {
            switch (battleManager.currentState)
            {
                case BattleState.PlayerTurn:
                    turnIndicator.text = "玩家回合";
                    turnIndicator.color = Color.green;
                    break;
                case BattleState.EnemyTurn:
                    turnIndicator.text = "敌人回合";
                    turnIndicator.color = Color.red;
                    break;
                case BattleState.BattleEnd:
                    turnIndicator.text = "战斗结束";
                    turnIndicator.color = Color.yellow;
                    break;
                default:
                    turnIndicator.text = "等待战斗";
                    turnIndicator.color = Color.gray;
                    break;
            }
        }

        if (playerTurnPanel != null)
            playerTurnPanel.SetActive(battleManager.currentState == BattleState.PlayerTurn);
        
        if (enemyTurnPanel != null)
            enemyTurnPanel.SetActive(battleManager.currentState == BattleState.EnemyTurn);
    }

    void OnAttackMenu()
    {
        if (battleManager.currentState != BattleState.PlayerTurn) return;
        attackPanel.SetActive(true);
    }

    void OnSkillButton(int skillIndex)
    {
        if (battleManager.currentState != BattleState.PlayerTurn) return;
        attackPanel.SetActive(false);
        battleManager.PlayerAction("skill", skillIndex);
    }

    void OnDefendMenu()
    {
        if (battleManager.currentState != BattleState.PlayerTurn) return;
        battleManager.PlayerAction("defend");
    }

    void OnItemMenu()
    {
        if (battleManager.currentState != BattleState.PlayerTurn) return;
        battleManager.PlayerAction("item");
    }

    void OnRunMenu()
    {
        if (battleManager.currentState != BattleState.PlayerTurn) return;
        battleManager.PlayerAction("run");
    }

    void OnCloseAttackMenu()
    {
        attackPanel.SetActive(false);
    }

    public void OnNormalAttack()
    {
        if (battleManager.currentState != BattleState.PlayerTurn) return;
        attackPanel.SetActive(false);
        battleManager.PlayerAction("attack");
    }

    public void SetButtonsInteractable(bool interactable)
    {
        attackMenu.interactable = interactable;
        defendMenu.interactable = interactable;
        itemMenu.interactable = interactable;
        runMenu.interactable = interactable;
        skill1Button.interactable = interactable;
        skill2Button.interactable = interactable;
        skill3Button.interactable = interactable;
        skill4Button.interactable = interactable;
    }
}