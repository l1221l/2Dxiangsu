using System;
using UnityEngine;

/// <summary>
/// 战斗单位类 - 表示战斗中的一个角色（玩家或敌人）
/// </summary>
[Serializable]
public class BattleUnit
{
    [Header("基础信息")]
    public string Name;           // 单位名称
    public bool IsPlayer;         // 是否为玩家单位

    [Header("战斗属性")]
    public int MaxHP;             // 最大生命值
    public int CurrentHP;         // 当前生命值
    public int Attack;            // 攻击力
    public int Defense;           // 防御力

    /// <summary>
    /// 是否存活
    /// </summary>
    public bool IsAlive => CurrentHP > 0;

    /// <summary>
    /// 生命值百分比 (0-1)
    /// </summary>
    public float HPPercent => (float)CurrentHP / MaxHP;

    /// <summary>
    /// 受到伤害
    /// </summary>
    public void TakeDamage(int damage)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - damage);
    }

    /// <summary>
    /// 恢复生命值
    /// </summary>
    public void Heal(int amount)
    {
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
    }
}
