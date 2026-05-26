using UnityEngine;

/// <summary>
/// 敌人控制器 - 管理地图上的敌人
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("敌人属性")]
    public string EnemyName = "怪物";
    public int MaxHP = 50;
    public int CurrentHP;
    public int Attack = 8;
    public int Defense = 3;
    public int ExpReward = 10;

    [Header("移动设置")]
    public bool CanMove = true;
    public float MoveSpeed = 2f;
    public float MoveInterval = 2f; // 移动间隔
    public float MoveRange = 3f; // 移动范围

    [Header("检测设置")]
    public float DetectionRange = 2f; // 检测玩家的范围
    public LayerMask PlayerLayer;

    private Vector2 _startPosition;
    private Vector2 _targetPosition;
    private float _moveTimer;
    private bool _isChasingPlayer = false;
    private Transform _playerTransform;
    private bool _isInBattle = false;

    void Start()
    {
        CurrentHP = MaxHP;
        _startPosition = transform.position;
        _targetPosition = _startPosition;
        
        // 查找玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (_isInBattle || GameManager.Instance.CurrentState != GameState.Exploration)
            return;

        // 检测玩家
        DetectPlayer();

        // 移动逻辑
        if (CanMove && !_isInBattle)
        {
            HandleMovement();
        }
    }

    /// <summary>
    /// 检测玩家
    /// </summary>
    void DetectPlayer()
    {
        if (_playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);
        
        if (distanceToPlayer <= DetectionRange)
        {
            // 进入战斗
            StartBattle();
        }
    }

    /// <summary>
    /// 开始战斗
    /// </summary>
    void StartBattle()
    {
        if (_isInBattle) return;
        
        _isInBattle = true;
        BattleManager.Instance?.StartBattle(this);
    }

    /// <summary>
    /// 处理移动
    /// </summary>
    void HandleMovement()
    {
        _moveTimer += Time.deltaTime;

        if (_moveTimer >= MoveInterval)
        {
            _moveTimer = 0f;
            
            // 随机选择新位置
            float randomX = Random.Range(-MoveRange, MoveRange);
            float randomY = Random.Range(-MoveRange, MoveRange);
            _targetPosition = _startPosition + new Vector2(randomX, randomY);
        }

        // 平滑移动
        transform.position = Vector2.MoveTowards(
            transform.position, 
            _targetPosition, 
            MoveSpeed * Time.deltaTime
        );
    }

    void OnDrawGizmosSelected()
    {
        // 绘制检测范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, DetectionRange);
        
        // 绘制移动范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? _startPosition : (Vector2)transform.position, MoveRange);
    }
}
