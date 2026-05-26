using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 战斗管理器 - 回合制战斗系统
/// 持久化单例，跨场景存在
/// BattleUI 由 SceneUI 动态创建并设置
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

    [Header("战斗UI（由 SceneUI 动态设置）")]
    public BattleUI BattleUI;

    private EnemyController _currentEnemy;

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

    public void StartBattle(EnemyController enemy)
    {
        if (IsInBattle) return;

        _currentEnemy = enemy;
        IsInBattle = true;
        
        GameManager.Instance.SetGameState(GameState.Battle);
        InitializeBattleUnits();
        BattleUI?.ShowBattleUI();
        StartCoroutine(BattleLoop());
    }

    void InitializeBattleUnits()
    {
        PlayerUnit = new BattleUnit
        {
            Name = "玩家",
            MaxHP = GameManager.Instance.PlayerData.MaxHP,
            CurrentHP = GameManager.Instance.PlayerData.CurrentHP,
            Attack = GameManager.Instance.PlayerData.Attack,
            Defense = GameManager.Instance.PlayerData.Defense,
            IsPlayer = true
        };

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

        BattleUI?.UpdateUI(PlayerUnit, EnemyUnit);
    }

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

    IEnumerator PlayerTurn()
    {
        BattleUI?.ShowActionMenu(true);
        BattleUI?.ShowMessage("玩家回合 - 请选择行动");

        bool actionTaken = false;
        while (!actionTaken)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                yield return StartCoroutine(PlayerAttack());
                actionTaken = true;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                yield return StartCoroutine(UseItem());
                actionTaken = true;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                yield return StartCoroutine(TryEscape());
                actionTaken = true;
            }
            yield return null;
        }

        BattleUI?.ShowActionMenu(false);

        if (CheckBattleEnd())
            CurrentState = BattleState.BattleEnd;
        else
            CurrentState = BattleState.EnemyTurn;
    }

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

    IEnumerator UseItem()
    {
        var consumables = GameManager.Instance.PlayerData.Inventory.FindAll(i => i.Type == ItemType.Consumable);
        
        if (consumables.Count > 0)
        {
            Item item = consumables[0];
            GameManager.Instance.PlayerData.RemoveItem(item);
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

    IEnumerator TryEscape()
    {
        BattleUI?.ShowMessage("尝试逃跑...");
        yield return new WaitForSeconds(0.5f);

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

    IEnumerator EnemyTurn()
    {
        BattleUI?.ShowMessage($"{EnemyUnit.Name} 的回合!");
        yield return new WaitForSeconds(0.5f);

        int damage = CalculateDamage(EnemyUnit, PlayerUnit);
        PlayerUnit.TakeDamage(damage);
        GameManager.Instance.PlayerData.TakeDamage(damage);
        
        BattleUI?.ShowMessage($"{EnemyUnit.Name} 造成 {damage} 点伤害!");
        BattleUI?.UpdateUI(PlayerUnit, EnemyUnit);
        
        yield return new WaitForSeconds(1f);

        if (CheckBattleEnd())
            CurrentState = BattleState.BattleEnd;
        else
            CurrentState = BattleState.PlayerTurn;
    }

    int CalculateDamage(BattleUnit attacker, BattleUnit defender)
    {
        int baseDamage = attacker.Attack;
        int damage = Mathf.Max(1, baseDamage - defender.Defense / 2);
        damage = Mathf.RoundToInt(damage * Random.Range(0.8f, 1.2f));
        return damage;
    }

    bool CheckBattleEnd()
    {
        return PlayerUnit.CurrentHP <= 0 || EnemyUnit.CurrentHP <= 0;
    }

    IEnumerator EndBattle()
    {
        bool playerWon = PlayerUnit.CurrentHP > 0;
        
        if (playerWon)
        {
            BattleUI?.ShowMessage("战斗胜利!");
            int expGain = _currentEnemy?.ExpReward ?? 10;
            GameManager.Instance.PlayerData.Exp += expGain;
            BattleUI?.ShowMessage($"获得 {expGain} 点经验值!");
            
            if (_currentEnemy != null)
                Destroy(_currentEnemy.gameObject);
        }
        else
        {
            BattleUI?.ShowMessage("战斗失败...");
            GameManager.Instance.SetGameState(GameState.GameOver);
        }

        yield return new WaitForSeconds(2f);
        BattleUI?.HideBattleUI();
        
        IsInBattle = false;
        CurrentState = BattleState.None;
        
        if (playerWon)
            GameManager.Instance.SetGameState(GameState.Exploration);
    }

    void EndBattle(bool playerWon)
    {
        StopAllCoroutines();
        BattleUI?.HideBattleUI();
        IsInBattle = false;
        CurrentState = BattleState.None;
        GameManager.Instance.SetGameState(GameState.Exploration);
    }
}
