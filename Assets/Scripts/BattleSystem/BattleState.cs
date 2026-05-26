using System;

/// <summary>
/// 战斗状态枚举
/// </summary>
public enum BattleState
{
    None,           // 无战斗
    PlayerTurn,     // 玩家回合
    EnemyTurn,      // 敌人回合
    BattleEnd       // 战斗结束
}
