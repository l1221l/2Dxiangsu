using UnityEngine;

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
