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
    public int MaxMP;
    public int CurrentMP;
    public int Attack;
    public int Defense;
    public int Speed; // 速度，影响行动顺序
    public bool IsPlayer;

    // 状态效果
    public bool isDefending = false;
    public bool isStunned = false;
    public int defenseBonus = 0;

    public void TakeDamage(int damage)
    {
        if (isDefending)
            damage = Mathf.Max(1, damage / 2); // 防御状态减半伤害
        
        CurrentHP -= damage;
        CurrentHP = Mathf.Max(0, CurrentHP);
    }

    public void Heal(int amount)
    {
        CurrentHP += amount;
        CurrentHP = Mathf.Min(CurrentHP, MaxHP);
    }

    public void ConsumeMP(int amount)
    {
        CurrentMP -= amount;
        CurrentMP = Mathf.Max(0, CurrentMP);
    }

    public void RestoreMP(int amount)
    {
        CurrentMP += amount;
        CurrentMP = Mathf.Min(CurrentMP, MaxMP);
    }

    public void StartDefend()
    {
        isDefending = true;
        defenseBonus = Defense / 2;
    }

    public void EndDefend()
    {
        isDefending = false;
        defenseBonus = 0;
    }

    public bool IsAlive()
    {
        return CurrentHP > 0;
    }

    public float GetHpPercentage()
    {
        return (float)CurrentHP / MaxHP;
    }

    public float GetMpPercentage()
    {
        return (float)CurrentMP / MaxMP;
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
