using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗管理器 - 回合制战斗系统
/// </summary>
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("战斗状态")]
    public bool IsInBattle = false;
    public BattleState CurrentState = BattleState.None;

    [Header("战斗单位")]
    public BattleUnit PlayerUnit;
    public BattleUnit EnemyUnit;

    [Header("战斗UI")]
    public BattleUI BattleUI;

    [Header("战斗场景")]
    public GameObject BattleScene; // 战斗场景画布/相机

    private EnemyController _currentEnemy; // 当前战斗的敌人

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 开始战斗
    /// </summary>
    public void StartBattle(EnemyController enemy)
    {
        if (IsInBattle) return;

        _currentEnemy = enemy;
        IsInBattle = true;
        
        // 切换到战斗状态
        GameManager.Instance.SetGameState(GameState.Battle);
        
        // 显示战斗场景
        if (BattleScene != null)
            BattleScene.SetActive(true);

        // 初始化战斗单位
        InitializeBattleUnits();

        // 显示战斗UI
        BattleUI?.ShowBattleUI();

        // 开始回合
        StartCoroutine(BattleLoop());
    }

    /// <summary>
    /// 初始化战斗单位
    /// </summary>
    void InitializeBattleUnits()
    {
        // 玩家单位
        PlayerUnit = new BattleUnit
        {
            Name = "玩家",
            MaxHP = GameManager.Instance.PlayerData.MaxHP,
            CurrentHP = GameManager.Instance.PlayerData.CurrentHP,
            Attack = GameManager.Instance.PlayerData.Attack,
            Defense = GameManager.Instance.PlayerData.Defense,
            IsPlayer = true
        };

        // 敌人单位
        if (_currentEnemy != null)
        {
            EnemyUnit = new BattleUnit
            {
                Name = _currentEnemy.EnemyName,
                MaxHP = _currentEnemy.MaxHP,
                CurrentHP = _currentEnemy.CurrentHP,
                Attack = _currentEnemy.Attack,
                Defense = _currentEnemy.Defense,
                IsPlayer = false
            };
        }

        // 更新UI
        BattleUI?.UpdateUI(PlayerUnit, EnemyUnit);
    }

    /// <summary>
    /// 战斗主循环
    /// </summary>
    IEnumerator BattleLoop()
    {
        CurrentState = BattleState.PlayerTurn;

        while (IsInBattle)
        {
            switch (CurrentState)
            {
                case BattleState.PlayerTurn:
                    yield return StartCoroutine(PlayerTurn());
                    break;
                case BattleState.EnemyTurn:
                    yield return StartCoroutine(EnemyTurn());
                    break;
                case BattleState.BattleEnd:
                    yield return StartCoroutine(EndBattle());
                    break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// 玩家回合
    /// </summary>
    IEnumerator PlayerTurn()
    {
        BattleUI?.ShowActionMenu(true);
        BattleUI?.ShowMessage("玩家回合 - 请选择行动");

        // 等待玩家输入
        bool actionTaken = false;
        while (!actionTaken)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                // 攻击
                yield return StartCoroutine(PlayerAttack());
                actionTaken = true;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                // 使用物品
                yield return StartCoroutine(UseItem());
                actionTaken = true;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                // 逃跑
                yield return StartCoroutine(TryEscape());
                actionTaken = true;
            }
            yield return null;
        }

        BattleUI?.ShowActionMenu(false);

        // 检查战斗结束
        if (CheckBattleEnd())
        {
            CurrentState = BattleState.BattleEnd;
        }
        else
        {
            CurrentState = BattleState.EnemyTurn;
        }
    }

    /// <summary>
    /// 玩家攻击
    /// </summary>
    IEnumerator PlayerAttack()
    {
        BattleUI?.ShowMessage($"{PlayerUnit.Name} 发起攻击!");
        yield return new WaitForSeconds(0.5f);

        int damage = CalculateDamage(PlayerUnit, EnemyUnit);
        EnemyUnit.TakeDamage(damage);
        
        BattleUI?.ShowMessage($"造成 {damage} 点伤害!");
        BattleUI?.UpdateUI(PlayerUnit, EnemyUnit);
        
        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// 使用物品
    /// </summary>
    IEnumerator UseItem()
    {
        // 简化版：直接使用第一个消耗品
        var consumables = GameManager.Instance.PlayerData.Inventory.FindAll(i => i.Type == ItemType.Consumable);
        
        if (consumables.Count > 0)
        {
            Item item = consumables[0];
            GameManager.Instance.PlayerData.RemoveItem(item);
            
            // 恢复生命值
            int healAmount = item.Value;
            PlayerUnit.Heal(healAmount);
            GameManager.Instance.PlayerData.Heal(healAmount);
            
            BattleUI?.ShowMessage($"使用了 {item.ItemName}, 恢复 {healAmount} 点生命值!");
            BattleUI?.UpdateUI(PlayerUnit, EnemyUnit);
        }
        else
        {
            BattleUI?.ShowMessage("没有可用的消耗品!");
        }
        
        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// 尝试逃跑
    /// </summary>
    IEnumerator TryEscape()
    {
        BattleUI?.ShowMessage("尝试逃跑...");
        yield return new WaitForSeconds(0.5f);

        // 50%逃跑成功率
        if (Random.value > 0.5f)
        {
            BattleUI?.ShowMessage("逃跑成功!");
            yield return new WaitForSeconds(1f);
            EndBattle(false);
        }
        else
        {
            BattleUI?.ShowMessage("逃跑失败!");
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// 敌人回合
    /// </summary>
    IEnumerator EnemyTurn()
    {
        BattleUI?.ShowMessage($"{EnemyUnit.Name} 的回合!");
        yield return new WaitForSeconds(0.5f);

        // 敌人攻击
        int damage = CalculateDamage(EnemyUnit, PlayerUnit);
        PlayerUnit.TakeDamage(damage);
        GameManager.Instance.PlayerData.TakeDamage(damage);
        
        BattleUI?.ShowMessage($"{EnemyUnit.Name} 造成 {damage} 点伤害!");
        BattleUI?.UpdateUI(PlayerUnit, EnemyUnit);
        
        yield return new WaitForSeconds(1f);

        // 检查战斗结束
        if (CheckBattleEnd())
        {
            CurrentState = BattleState.BattleEnd;
        }
        else
        {
            CurrentState = BattleState.PlayerTurn;
        }
    }

    /// <summary>
    /// 计算伤害
    /// </summary>
    int CalculateDamage(BattleUnit attacker, BattleUnit defender)
    {
        int baseDamage = attacker.Attack;
        int damage = Mathf.Max(1, baseDamage - defender.Defense / 2);
        
        // 随机波动 (80% - 120%)
        damage = Mathf.RoundToInt(damage * Random.Range(0.8f, 1.2f));
        
        return damage;
    }

    /// <summary>
    /// 检查战斗是否结束
    /// </summary>
    bool CheckBattleEnd()
    {
        return PlayerUnit.CurrentHP <= 0 || EnemyUnit.CurrentHP <= 0;
    }

    /// <summary>
    /// 结束战斗
    /// </summary>
    IEnumerator EndBattle()
    {
        bool playerWon = PlayerUnit.CurrentHP > 0;
        
        if (playerWon)
        {
            BattleUI?.ShowMessage("战斗胜利!");
            
            // 获得经验值和奖励
            int expGain = _currentEnemy?.ExpReward ?? 10;
            GameManager.Instance.PlayerData.Exp += expGain;
            BattleUI?.ShowMessage($"获得 {expGain} 点经验值!");
            
            // 销毁敌人
            if (_currentEnemy != null)
            {
                Destroy(_currentEnemy.gameObject);
            }
        }
        else
        {
            BattleUI?.ShowMessage("战斗失败...");
            GameManager.Instance.SetGameState(GameState.GameOver);
        }

        yield return new WaitForSeconds(2f);

        // 隐藏战斗场景
        if (BattleScene != null)
            BattleScene.SetActive(false);

        BattleUI?.HideBattleUI();
        
        IsInBattle = false;
        CurrentState = BattleState.None;
        
        // 返回探索模式
        if (playerWon)
        {
            GameManager.Instance.SetGameState(GameState.Exploration);
        }
    }

    /// <summary>
    /// 强制结束战斗（用于逃跑）
    /// </summary>
    void EndBattle(bool playerWon)
    {
        StopAllCoroutines();
        
        if (!playerWon && _currentEnemy != null)
        {
            // 逃跑时敌人保留
        }

        if (BattleScene != null)
            BattleScene.SetActive(false);

        BattleUI?.HideBattleUI();
        
        IsInBattle = false;
        CurrentState = BattleState.None;
        GameManager.Instance.SetGameState(GameState.Exploration);
    }
}

/// <summary>
/// 战斗单位数据
/// </summary>
[System.Serializable]
public class BattleUnit
{
    public string Name;
    public int MaxHP;
    public int CurrentHP;
    public int Attack;
    public int Defense;
    public bool IsPlayer;

    public void TakeDamage(int damage)
    {
        CurrentHP -= damage;
        CurrentHP = Mathf.Max(0, CurrentHP);
    }

    public void Heal(int amount)
    {
        CurrentHP += amount;
        CurrentHP = Mathf.Min(CurrentHP, MaxHP);
    }
}

/// <summary>
/// 战斗状态
/// </summary>
public enum BattleState
{
    None,
    PlayerTurn,
    EnemyTurn,
    BattleEnd
}
