using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏管理器 - 单例模式
/// 管理游戏全局状态、玩家数据、场景切换
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("游戏状态")]
    public GameState CurrentState = GameState.Exploration;
    public int CurrentFloor = 0; // 0=屋外, 1=屋内一层, 2=BOSS房

    [Header("玩家数据")]
    public PlayerData PlayerData = new PlayerData();

    [Header("场景名称")]
    public string[] FloorScenes = { "Outside", "Floor1", "BossRoom" };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeGame()
    {
        PlayerData.Initialize();
        Debug.Log("游戏初始化完成");
    }

    /// <summary>
    /// 切换游戏状态
    /// </summary>
    public void SetGameState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"游戏状态切换为: {newState}");
    }

    /// <summary>
    /// 进入下一层
    /// </summary>
    public void GoToNextFloor()
    {
        CurrentFloor++;
        if (CurrentFloor < FloorScenes.Length)
        {
            LoadFloor(CurrentFloor);
        }
        else
        {
            Debug.Log("已到达最高层！");
        }
    }

    /// <summary>
    /// 加载指定楼层
    /// </summary>
    public void LoadFloor(int floorIndex)
    {
        if (floorIndex >= 0 && floorIndex < FloorScenes.Length)
        {
            CurrentFloor = floorIndex;
            SceneManager.LoadScene(FloorScenes[floorIndex]);
        }
    }

    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public void RestartGame()
    {
        CurrentFloor = 0;
        PlayerData.Initialize();
        LoadFloor(0);
    }
}

/// <summary>
/// 游戏状态枚举
/// </summary>
public enum GameState
{
    Exploration,    // 探索模式
    Battle,         // 战斗模式
    Dialogue,       // 对话模式
    Menu,           // 菜单模式
    GameOver        // 游戏结束
}

/// <summary>
/// 玩家数据类
/// </summary>
[System.Serializable]
public class PlayerData
{
    public int MaxHP = 100;
    public int CurrentHP = 100;
    public int Attack = 10;
    public int Defense = 5;
    public int Level = 1;
    public int Exp = 0;
    public List<Item> Inventory = new List<Item>();

    public void Initialize()
    {
        MaxHP = 100;
        CurrentHP = 100;
        Attack = 10;
        Defense = 5;
        Level = 1;
        Exp = 0;
        Inventory.Clear();
    }

    public void TakeDamage(int damage)
    {
        int actualDamage = Mathf.Max(1, damage - Defense);
        CurrentHP -= actualDamage;
        CurrentHP = Mathf.Max(0, CurrentHP);
    }

    public void Heal(int amount)
    {
        CurrentHP += amount;
        CurrentHP = Mathf.Min(CurrentHP, MaxHP);
    }

    public void AddItem(Item item)
    {
        Inventory.Add(item);
    }

    public void RemoveItem(Item item)
    {
        Inventory.Remove(item);
    }
}
