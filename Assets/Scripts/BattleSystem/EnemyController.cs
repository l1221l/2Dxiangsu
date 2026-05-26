using UnityEngine;

/// <summary>
/// 敌人控制器 - 挂载在敌人GameObject上
/// 负责碰撞检测、触发战斗、敌人属性配置
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("敌人属性")]
    public string enemyName = "史莱姆";
    public int maxHP = 50;
    public int currentHP = 50;
    public int maxMP = 20;
    public int currentMP = 20;
    public int attack = 8;
    public int defense = 3;
    public int speed = 5;
    public int expReward = 15;

    [Header("战斗配置")]
    public bool isBoss = false;
    public bool destroyOnDefeat = true;

    [Header("动画")]
    public Animator animator;
    public string idleAnim = "Idle";
    public string attackAnim = "Attack";

    [Header("移动（可选）")]
    public bool canMove = false;
    public float moveSpeed = 2f;
    public float moveRange = 3f;
    public float moveInterval = 2f;

    private Rigidbody2D rb;
    private Vector2 startPosition;
    private float moveTimer;
    private bool isDefeated = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true; // 不受物理影响，只用于碰撞检测
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (animator == null)
            animator = GetComponent<Animator>();

        startPosition = transform.position;
        moveTimer = Random.Range(0f, moveInterval);
    }

    void Start()
    {
        // 确保碰撞器是触发器
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    void Update()
    {
        if (isDefeated) return;

        // 播放待机动画
        if (animator != null && !string.IsNullOrEmpty(idleAnim))
            animator.Play(idleAnim);

        // 敌人移动（如果启用）
        if (canMove)
            EnemyMove();
    }

    /// <summary>
    /// 敌人巡逻移动
    /// </summary>
    void EnemyMove()
    {
        moveTimer -= Time.deltaTime;
        if (moveTimer <= 0)
        {
            moveTimer = moveInterval;
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            Vector2 targetPos = startPosition + randomDir * Random.Range(0, moveRange);
            
            // 简单移动（实际项目中可用Lerp或寻路）
            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * moveInterval);
        }
    }

    /// <summary>
    /// 碰撞检测 - 玩家进入触发器范围
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDefeated) return;

        // 检测是否是玩家
        if (other.CompareTag("Player"))
        {
            TriggerBattle();
        }
    }

    /// <summary>
    /// 碰撞持续检测（防止快速穿过）
    /// </summary>
    void OnTriggerStay2D(Collider2D other)
    {
        if (isDefeated) return;

        if (other.CompareTag("Character"))
        {
            // 如果不在战斗中且游戏状态允许，触发战斗
            if (GameManager.Instance.CurrentState == GameState.Exploration)
            {
                TriggerBattle();
            }
        }
    }

    /// <summary>
    /// 触发战斗
    /// </summary>
    void TriggerBattle()
    {
        if (BattleManager.Instance == null)
        {
            Debug.LogError("BattleManager 未找到！请确保场景中有 BattleManager。");
            return;
        }

        if (BattleManager.Instance.isBattleActive)
        {
            Debug.Log("已经在战斗中，无法触发新战斗。");
            return;
        }

        Debug.Log($"与 {enemyName} 触发战斗！");

        // 切换游戏状态
        GameManager.Instance.SetGameState(GameState.Battle);

        // 构建玩家战斗单位
        BattleUnit playerUnit = new BattleUnit
        {
            Name = "玩家",
            MaxHP = GameManager.Instance.PlayerData.MaxHP,
            CurrentHP = GameManager.Instance.PlayerData.CurrentHP,
            MaxMP = 50,
            CurrentMP = 50,
            Attack = GameManager.Instance.PlayerData.Attack,
            Defense = GameManager.Instance.PlayerData.Defense,
            Speed = 10,
            IsPlayer = true
        };

        // 构建敌人战斗单位
        BattleUnit enemyUnit = new BattleUnit
        {
            Name = enemyName,
            MaxHP = maxHP,
            CurrentHP = currentHP,
            MaxMP = maxMP,
            CurrentMP = currentMP,
            Attack = attack,
            Defense = defense,
            Speed = speed,
            IsPlayer = false
        };

        // 开始战斗
        BattleManager.Instance.StartBattle(playerUnit, enemyUnit, this);
    }

    /// <summary>
    /// 战斗胜利后调用
    /// </summary>
    public void OnBattleWon()
    {
        isDefeated = true;

        // 给予经验值
        GameManager.Instance.PlayerData.Exp += expReward;
        Debug.Log($"击败 {enemyName}，获得 {expReward} 经验值！");

        if (destroyOnDefeat)
        {
            // 播放死亡动画后销毁
            StartCoroutine(DeathSequence());
        }
    }

    /// <summary>
    /// 死亡序列
    /// </summary>
    System.Collections.IEnumerator DeathSequence()
    {
        // 可以在这里播放死亡动画
        if (animator != null)
            animator.Play("Death");

        yield return new WaitForSeconds(0.5f);

        // 禁用碰撞器防止重复触发
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // 淡出效果
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            float fadeTime = 0.5f;
            float elapsed = 0f;
            Color originalColor = sr.color;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// 播放攻击动画
    /// </summary>
    public void PlayAttackAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(attackAnim))
            animator.Play(attackAnim);
    }

    /// <summary>
    /// 在编辑器中显示巡逻范围
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (canMove)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : (Vector2)transform.position, moveRange);
        }
    }
}