using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 战斗管理器 - 控制回合流程、技能释放、胜负判断
/// </summary>
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("战斗状态")]
    public BattleState currentState = BattleState.None;
    public bool isBattleActive = false;

    [Header("战斗单位")]
    public BattleUnit playerUnit;
    public BattleUnit enemyUnit;

    [Header("UI引用")]
    public BattleUI battleUI;
    public BattleEmenuCtrl battleMenu;

    [Header("动画控制器")]
    public Animator playerAnimator;
    public Animator enemyAnimator;

    [Header("战斗参数")]
    public float turnDelay = 1f; // 回合间延迟
    public float skillCastDelay = 0.5f; // 技能释放延迟

    [SerializeField] private List<SkillData> playerSkills = new List<SkillData>();
    [SerializeField] private List<SkillData> enemySkills = new List<SkillData>();
    private EnemyController currentEnemyController;
    private GameObject battleBgInstance; // 运行时实例化的战斗UI
    private SpriteSwitcher playerSwitcher;
    private SpriteSwitcher enemySwitcher;
    private BattleEnemyAnimator enemyAnim;

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
            return;
        }

    }

    /// <summary>
    /// 开始战斗
    /// </summary>
    public void StartBattle(BattleUnit player, BattleUnit enemy, EnemyController enemyCtrl = null)
    {
        if (isBattleActive) return;

        playerUnit = player;
        enemyUnit = enemy;
        currentEnemyController = enemyCtrl;
        isBattleActive = true;
        currentState = BattleState.PlayerTurn;

        // 从Resources加载战斗UI预制体
        LoadBattleUI();

        // 初始化技能
        InitializeSkills();

        // 更新UI
        UpdateUI();

        // 显示战斗UI
        if (battleUI != null)
            battleUI.ShowBattlePanel();
        if (battleMenu != null)
            battleMenu.gameObject.SetActive(true);

        // 播放进入战斗动画
        StartCoroutine(PlayBattleStartAnimation());

        Debug.Log("战斗开始！");
    }

    /// <summary>
    /// 从Resources加载并实例化战斗UI
    /// </summary>
    void LoadBattleUI()
    {
        GameObject prefab = Resources.Load<GameObject>("BattleBg");
        if (prefab == null)
        {
            Debug.LogError("BattleManager: Resources/BattleBg 预制体不存在！");
            return;
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("BattleManager: 场景中没有Canvas！");
            return;
        }

        battleBgInstance = Instantiate(prefab, canvas.transform);
        battleBgInstance.name = "BattleBg";
        battleBgInstance.SetActive(true); // 预制体默认隐藏，强制激活

        // 获取组件
        battleUI = battleBgInstance.GetComponentInChildren<BattleUI>(true);
        battleMenu = battleBgInstance.GetComponentInChildren<BattleEmenuCtrl>(true);

        // 在PlayerPos/EnemyPos创建角色Image并设置精灵图
        CreateBattleCharacterImages();

        Debug.Log($"BattleManager: 战斗UI加载完成 UI={battleUI != null} Menu={battleMenu != null}");

        // 查找动画控制器
        if (playerAnimator == null)
            playerAnimator = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Animator>();
        if (enemyAnimator == null)
            enemyAnimator = GameObject.FindGameObjectWithTag("Enemy")?.GetComponent<Animator>();
    }

    /// <summary>
    /// 在 PlayerPos / EnemyPos 位置实例化角色和Boss预制体，附加SpriteSwitcher
    /// </summary>
    void CreateBattleCharacterImages()
    {
        if (battleUI == null || battleBgInstance == null) return;

        Transform playerPos = null;
        Transform enemyPos = null;

        // 查找 PlayerPos
        playerPos = battleBgInstance.transform.Find("Player/PlayerPos");
        if (playerPos == null)
        {
            Debug.LogWarning("BattleManager: Player/PlayerPos 未找到，尝试其他命名");
            playerPos = battleBgInstance.transform.Find("Player/Player.Pos");
        }

        // 查找 EnemyPos
        enemyPos = battleBgInstance.transform.Find("Enemy/EnemyPos");
        if (enemyPos == null)
            enemyPos = battleBgInstance.transform.Find("Enemy/Enemy.Pos");

        if (playerPos != null)
        {
            GameObject charPrefab = Resources.Load<GameObject>("character");
            if (charPrefab != null)
            {
                GameObject charInstance = Instantiate(charPrefab, playerPos);
                charInstance.name = "character";
                battleUI.playerImage = charInstance.GetComponent<Image>();
                playerSwitcher = charInstance.AddComponent<SpriteSwitcher>();
                Debug.Log("BattleManager: character 预制体已实例化到 PlayerPos");
            }
            else Debug.LogError("BattleManager: Resources/character 预制体不存在！");
        }
        else Debug.LogWarning("BattleManager: 未找到 PlayerPos，无法实例化角色");

        if (enemyPos != null)
        {
            GameObject bossPrefab = Resources.Load<GameObject>("Boss");
            if (bossPrefab != null)
            {
                GameObject bossInstance = Instantiate(bossPrefab, enemyPos);
                bossInstance.name = "Boss";
                battleUI.enemyImage = bossInstance.GetComponent<Image>();
                enemySwitcher = bossInstance.GetComponent<SpriteSwitcher>();
                BattleEnemyAnimator anim = bossInstance.GetComponent<BattleEnemyAnimator>();
                
                if (anim != null)
                {
                    anim.Init(enemyPos, playerPos);
                    enemyAnim = anim;
                }
                else
                {
                    Debug.LogWarning("BattleManager: Boss 预制体上未找到 BattleEnemyAnimator 组件");
                }
                Debug.Log("BattleManager: Boss 预制体已实例化到 EnemyPos，动画控制器已初始化");
            }
            else Debug.LogError("BattleManager: Resources/Boss 预制体不存在！");
        }
        else Debug.LogWarning("BattleManager: 未找到 EnemyPos，无法实例化 Boss");
    }

    /// <summary>
    /// 初始化技能
    /// </summary>
    void InitializeSkills()
    {
        // 只在 Inspector 没配置时才用默认值
        if (playerSkills.Count == 0)
        {
            playerSkills = new List<SkillData>
            {
                new SkillData { skillName = "火球术", damage = 15, mpCost = 10, isAOE = false },
                new SkillData { skillName = "治疗术", damage = -20, mpCost = 15, isAOE = false, isHealing = true },
                new SkillData { skillName = "闪电链", damage = 25, mpCost = 20, isAOE = true }
            };
        }

        if (enemySkills.Count == 0)
        {
            enemySkills = new List<SkillData>
            {
                new SkillData { skillName = "爪击", damage = 10, mpCost = 0, isAOE = false },
                new SkillData { skillName = "咆哮", damage = 5, mpCost = 5, isAOE = true }
            };
        }
    }

    /// <summary>
    /// 更新UI显示
    /// </summary>
    void UpdateUI()
    {
        if (battleUI != null)
        {
            // 更新玩家HP/MP
            battleUI.SetHp(playerUnit.CurrentHP, playerUnit.MaxHP);
            battleUI.SetMp(playerUnit.CurrentMP, playerUnit.MaxMP);

            // 更新敌人HP/MP
            battleUI.SetEnemyHp(enemyUnit.CurrentHP, enemyUnit.MaxHP);
            battleUI.SetEnemyMp(enemyUnit.CurrentMP, enemyUnit.MaxMP);
        }
    }

    /// <summary>
    /// 玩家回合 - 通过UI按钮调用
    /// </summary>
    public void PlayerAction(string actionType, int skillIndex = -1)
    {
        if (currentState != BattleState.PlayerTurn || !isBattleActive) return;

        // 禁用UI按钮
        if (battleMenu != null)
            battleMenu.SetButtonsInteractable(false);

        StartCoroutine(PlayerActionCoroutine(actionType, skillIndex));
    }

    IEnumerator PlayerActionCoroutine(string actionType, int skillIndex)
    {
        switch (actionType)
        {
            case "skill":
                if (skillIndex >= 0 && skillIndex < playerSkills.Count)
                    yield return StartCoroutine(UseSkill(playerUnit, enemyUnit, playerSkills[skillIndex]));
                break;
            case "defend":
                yield return StartCoroutine(PlayerDefend());
                break;
            case "item":
                yield return StartCoroutine(UseItem());
                break;
            case "run":
                yield return StartCoroutine(TryEscape());
                break;
        }

        // 检查战斗是否结束
        if (CheckBattleEnd())
        {
            yield return StartCoroutine(EndBattle());
        }
        else
        {
            // 切换到敌人回合
            currentState = BattleState.EnemyTurn;
            yield return new WaitForSeconds(turnDelay);
            StartCoroutine(EnemyTurn());
        }
    }

    /// <summary>
    /// 通用攻击命中特效：atkEffectPrefab 从 source 飞向 target → 爆炸 boomEffectPrefab
    /// </summary>
    IEnumerator PlayAttackHitEffect(Transform source, Transform target, GameObject atkEffectPrefab, GameObject boomEffectPrefab)
    {
        GameObject atkEffect = null;
        Transform parent = battleUI != null ? battleUI.transform : null;

        Debug.Log($"[PlayAttackHitEffect] source={source?.name}, target={target?.name}, atkPrefab={atkEffectPrefab?.name}, boomPrefab={boomEffectPrefab?.name}, parent={parent?.name}");

        if (atkEffectPrefab != null && source != null && target != null && parent != null)
        {
            Vector3 startLocal = parent.InverseTransformPoint(source.position);
            Vector3 endLocal = parent.InverseTransformPoint(target.position);

            Debug.Log($"[PlayAttackHitEffect] 实例化 atkEffect 于 {startLocal} -> {endLocal}");

            atkEffect = Instantiate(atkEffectPrefab, parent);
            atkEffect.transform.localPosition = startLocal;
            Debug.Log($"[PlayAttackHitEffect] atkEffect 实例化后 localPosition={atkEffect.transform.localPosition}, activeSelf={atkEffect.activeSelf}, name={atkEffect.name}");

            float duration = 0.4f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                atkEffect.transform.localPosition = Vector3.Lerp(startLocal, endLocal, t);
                yield return null;
            }

            atkEffect.transform.localPosition = endLocal;
            Debug.Log($"[PlayAttackHitEffect] atkEffect 到达，销毁");
            Destroy(atkEffect);
        }
        else
        {
            Debug.LogWarning($"[PlayAttackHitEffect] 条件不满足: atkPrefab={(atkEffectPrefab != null)}, source={(source != null)}, target={(target != null)}, parent={(parent != null)}");
        }

        if (boomEffectPrefab != null && target != null && parent != null)
        {
            Vector3 boomLocal = parent.InverseTransformPoint(target.position);
            Debug.Log($"[PlayAttackHitEffect] 实例化 boom 于 {boomLocal}");
            GameObject boomEffect = Instantiate(boomEffectPrefab, parent);
            boomEffect.transform.localPosition = boomLocal;
            Debug.Log($"[PlayAttackHitEffect] boom 实例化后 localPosition={boomEffect.transform.localPosition}, activeSelf={boomEffect.activeSelf}");
            yield return new WaitForSeconds(0.6f);
            Destroy(boomEffect);
        }
        else
        {
            Debug.LogWarning($"[PlayAttackHitEffect] boom条件不满足: boomPrefab={(boomEffectPrefab != null)}, target={(target != null)}, parent={(parent != null)}");
        }
    }

    /// <summary>
    /// 玩家防御
    /// </summary>
    IEnumerator PlayerDefend()
    {
        Debug.Log($"{playerUnit.Name} 进入防御状态！");

        // 增加防御力一回合
        playerUnit.Defense += 5;

        // 播放防御动画
        if (playerAnimator != null)
            playerAnimator.Play("Defend");

        yield return new WaitForSeconds(1f);

        // 下回合恢复防御力
        playerUnit.Defense -= 5;
    }

    /// <summary>
    /// 使用技能
    /// </summary>
    IEnumerator UseSkill(BattleUnit caster, BattleUnit target, SkillData skill)
    {
        if (caster.CurrentMP < skill.mpCost)
        {
            Debug.Log("MP不足，无法使用技能！");
            yield break;
        }

        Debug.Log($"{caster.Name} 使用 {skill.skillName}！");

        // 消耗MP
        caster.CurrentMP -= skill.mpCost;

        // 播放技能动画
        if (caster.IsPlayer && playerSwitcher != null)
            playerSwitcher.PlaySkillAnimation();
        else if (!caster.IsPlayer && enemyAnim != null)
            yield return enemyAnim.AttackSequence();
        else if (!caster.IsPlayer && enemyAnimator != null)
            enemyAnimator.Play("Attack");

        yield return new WaitForSeconds(skillCastDelay);

        // 播放技能特效（飞行 → 爆炸）
        if (!skill.isHealing)
        {
            Transform source = caster.IsPlayer ? battleUI.playerTransform : battleUI.enemyTransform;
            Transform dest = target.IsPlayer ? battleUI.playerTransform : battleUI.enemyTransform;
            Debug.Log($"[UseSkill] 即将播放特效: caster={caster.Name}, target={target.Name}, source={source?.name}, dest={dest?.name}, atkPrefab={skill.atkEffectPrefab?.name}, boomPrefab={skill.boomEffectPrefab?.name}");
            yield return StartCoroutine(PlayAttackHitEffect(source, dest, skill.atkEffectPrefab, skill.boomEffectPrefab));
        }

        // 处理技能效果
        if (skill.isHealing)
        {
            // 治疗技能
            caster.Heal(-skill.damage); // damage为负值表示治疗
            Debug.Log($"恢复 {-skill.damage} 点生命值！");
        }
        else
        {
            // 伤害技能（单体）
            int damage = CalculateSkillDamage(caster, target, skill);
            target.TakeDamage(damage);
            if (target == enemyUnit && enemySwitcher != null) enemySwitcher.TriggerBlink();
            else if (target == playerUnit && playerSwitcher != null) playerSwitcher.TriggerBlink();
            
            Debug.Log($"造成 {damage} 点技能伤害！");
        }

        UpdateUI();
        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// 使用道具
    /// </summary>
    IEnumerator UseItem()
    {
        Debug.Log("使用道具！");

        // 这里可以连接道具系统
        // 暂时简单治疗
        playerUnit.Heal(30);
        UpdateUI();

        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// 尝试逃跑
    /// </summary>
    IEnumerator TryEscape()
    {
        Debug.Log("尝试逃跑...");

        float escapeChance = 0.5f; // 50%逃跑成功率
        if (Random.value < escapeChance)
        {
            Debug.Log("逃跑成功！");
            yield return StartCoroutine(EndBattle(true));
        }
        else
        {
            Debug.Log("逃跑失败！");
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// 敌人回合
    /// </summary>
    IEnumerator EnemyTurn()
    {
        if (!isBattleActive || currentState != BattleState.EnemyTurn) yield break;

        Debug.Log($"{enemyUnit.Name} 的回合！");

        // AI：优先使用技能，否则普通攻击（始终有攻击动画）
        if (enemySkills.Count > 0 && enemyUnit.CurrentMP >= enemySkills[0].mpCost && Random.value > 0.4f)
        {
            int skillIndex = Random.Range(0, enemySkills.Count);
            yield return StartCoroutine(UseSkill(enemyUnit, playerUnit, enemySkills[skillIndex]));
        }
        else
        {
            yield return StartCoroutine(EnemyAttack());
        }

        // 检查战斗是否结束
        if (CheckBattleEnd())
        {
            yield return StartCoroutine(EndBattle());
        }
        else
        {
            // 切换到玩家回合
            currentState = BattleState.PlayerTurn;
            // 启用UI按钮
            if (battleMenu != null)
                battleMenu.SetButtonsInteractable(true);
            Debug.Log("轮到玩家回合！");
        }
    }

    /// <summary>
    /// 敌人普通攻击（带移动动画）
    /// </summary>
    IEnumerator EnemyAttack()
    {
        Debug.Log($"EnemyAttack: enemyAnim={enemyAnim}");

        // 如果有 BattleEnemyAnimator，执行移动攻击序列
        if (enemyAnim != null)
        {
            yield return enemyAnim.AttackSequence();
        }
        else
        {
            Debug.Log("EnemyAttack: 使用回退动画");
            if (enemyAnimator != null)
                enemyAnimator.Play("Attack");
            yield return new WaitForSeconds(0.3f);
        }

        // 计算伤害
        int damage = CalculateDamage(enemyUnit, playerUnit);
        playerUnit.TakeDamage(damage);
        if (playerSwitcher != null) playerSwitcher.TriggerBlink();

        Debug.Log($"{enemyUnit.Name} 造成 {damage} 点伤害！");
        UpdateUI();

        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// 计算普通攻击伤害
    /// </summary>
    int CalculateDamage(BattleUnit attacker, BattleUnit defender)
    {
        int baseDamage = attacker.Attack;
        int damage = Mathf.Max(1, baseDamage - defender.Defense / 2);
        damage = Mathf.RoundToInt(damage * Random.Range(0.8f, 1.2f)); // 20%浮动
        return damage;
    }

    /// <summary>
    /// 计算技能伤害
    /// </summary>
    int CalculateSkillDamage(BattleUnit attacker, BattleUnit defender, SkillData skill)
    {
        int baseDamage = skill.damage + attacker.Attack / 2;
        int damage = Mathf.Max(1, baseDamage - defender.Defense / 3);
        damage = Mathf.RoundToInt(damage * Random.Range(0.9f, 1.1f)); // 10%浮动
        return damage;
    }

    /// <summary>
    /// 检查战斗是否结束
    /// </summary>
    bool CheckBattleEnd()
    {
        if (playerUnit == null || enemyUnit == null) return true;
        return playerUnit.CurrentHP <= 0 || enemyUnit.CurrentHP <= 0;
    }

    /// <summary>
    /// 结束战斗
    /// </summary>
    IEnumerator EndBattle(bool escaped = false)
    {
        isBattleActive = false;
        currentState = BattleState.BattleEnd;

        if (escaped)
        {
            Debug.Log("成功逃脱战斗！");
        }
        else if (playerUnit.CurrentHP <= 0)
        {
            Debug.Log("战斗失败...");
            GameManager.Instance.SetGameState(GameState.GameOver);
        }
        else if (enemyUnit.CurrentHP <= 0)
        {
            Debug.Log("战斗胜利！");
            // 同步玩家HP回PlayerData
            GameManager.Instance.PlayerData.CurrentHP = playerUnit.CurrentHP;
            // 通知敌人被击败
            if (currentEnemyController != null)
                currentEnemyController.OnBattleWon();
        }

        // 恢复探索状态
        GameManager.Instance.SetGameState(GameState.Exploration);

        // 隐藏并销毁战斗UI
        if (battleBgInstance != null)
        {
            Destroy(battleBgInstance);
            battleBgInstance = null;
            battleUI = null;
            battleMenu = null;
        }

        yield return new WaitForSeconds(2f);

        // 清理战斗
        CleanupBattle();
    }

    /// <summary>
    /// 播放战斗开始动画
    /// </summary>
    IEnumerator PlayBattleStartAnimation()
    {
        // 可以在这里播放进入战斗的动画
        Debug.Log("战斗开始动画播放中...");
        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// 清理战斗
    /// </summary>
    void CleanupBattle()
    {
        playerUnit = null;
        enemyUnit = null;
        currentState = BattleState.None;
        
        Debug.Log("战斗清理完成");
    }

    /// <summary>
    /// 获取玩家技能列表
    /// </summary>
    public List<SkillData> GetPlayerSkills()
    {
        return playerSkills;
    }
}

/// <summary>
/// 技能数据
/// </summary>
[System.Serializable]
public class SkillData
{
    public string skillName;
    public int damage; // 负值表示治疗
    public int mpCost;
    public bool isAOE = false;
    public bool isHealing = false;
    public GameObject atkEffectPrefab;  // 飞行特效预制体（texiaoAtk）
    public GameObject boomEffectPrefab; // 爆炸特效预制体（Boom）
}