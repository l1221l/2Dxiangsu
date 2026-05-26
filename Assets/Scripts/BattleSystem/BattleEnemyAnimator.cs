using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 战斗中敌人动画控制器 - 挂载在 Boss 预制体实例上。
/// 敌人回合时：向玩家位置移动 → 播放攻击动画 → 回到原位。
/// </summary>
public class BattleEnemyAnimator : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 800f;         // 移动速度（像素/秒）
    public float attackHoldTime = 0.3f;    // 攻击位置停留时间
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("攻击动画")]
    public Sprite[] attackFrames;          // 攻击动画帧（可选）
    public float attackFrameRate = 10f;    // 攻击动画帧率

    private SpriteSwitcher switcher;
    private Image image;
    private RectTransform rectTransform;
    private Vector3 homeWorldPos;       // 世界坐标下的原位
    private Vector2 attackOffset;       // 世界坐标下的攻击偏移量

    void Awake()
    {
        image = GetComponent<Image>();
        switcher = GetComponent<SpriteSwitcher>();
        rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 初始化：传入 EnemyPos 和 PlayerPos，用世界坐标计算攻击偏移
    /// </summary>
    public void Init(Transform enemyPos, Transform playerPos)
    {
        if (rectTransform == null) return;

        homeWorldPos = rectTransform.position;

        if (enemyPos != null && playerPos != null)
        {
            // 玩家在敌人右侧，所以敌人应该向右移动（正X方向）
            Vector3 diff = playerPos.position - enemyPos.position;
            // 如果 diff.x 为正（玩家在右），attackOffset 应为正
            // 如果 diff.x 太小，用固定值
            float offsetX = Mathf.Abs(diff.x) > 10f ? diff.x * 0.5f : 200f;
            attackOffset = new Vector2(offsetX, 0f);
            Debug.Log($"BattleEnemyAnimator: homeWorld={homeWorldPos} enemyWorld={enemyPos.position} playerWorld={playerPos.position} diff={diff} offset={attackOffset}");
        }
        else
        {
            attackOffset = new Vector2(200f, 0f); // 向右移动
            Debug.LogWarning("BattleEnemyAnimator: 使用默认向右偏移");
        }
    }

    /// <summary>
    /// 递归累加 anchoredPosition 直到 Canvas，得到 Canvas 空间下绝对坐标
    /// </summary>
    Vector2 GetCanvasPosition(RectTransform rt)
    {
        Vector2 pos = rt.anchoredPosition;
        RectTransform parent = rt.parent as RectTransform;
        while (parent != null && parent.GetComponent<Canvas>() == null)
        {
            pos += parent.anchoredPosition;
            parent = parent.parent as RectTransform;
        }
        return pos;
    }

    public IEnumerator AttackSequence()
    {
        if (rectTransform == null)
        {
            Debug.LogWarning("BattleEnemyAnimator: rectTransform 为空");
            yield break;
        }

        // 阶段1：冲向玩家（世界坐标移动）
        yield return StartCoroutine(MoveByWorldOffset(attackOffset));

        // 阶段2：攻击动画
        yield return StartCoroutine(PlayAttackAnim());

        // 阶段3：退回原位（世界坐标移动）
        yield return StartCoroutine(MoveByWorldOffset(-attackOffset));
    }

    /// <summary>
    /// 按世界坐标偏移量移动
    /// </summary>
    IEnumerator MoveByWorldOffset(Vector2 worldOffset)
    {
        Vector3 start = rectTransform.position;
        Vector3 target = start + (Vector3)worldOffset;
        float distance = worldOffset.magnitude;
        float duration = distance / Mathf.Max(moveSpeed, 1f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rectTransform.position = Vector3.Lerp(start, target, moveCurve.Evaluate(t));
            yield return null;
        }
        rectTransform.position = target;
    }

    /// <summary>
    /// 播放攻击动画帧（如果有）并停留
    /// </summary>
    IEnumerator PlayAttackAnim()
    {
        // 如果有攻击帧且有 switcher，临时切换帧
        Sprite[] originalFrames = null;
        if (switcher != null && attackFrames != null && attackFrames.Length > 0)
        {
            originalFrames = switcher.frames;
            switcher.frames = attackFrames;
            try { switcher.StopCycle(); } catch { }
            try { switcher.StartCycle(); } catch { }
        }

        yield return new WaitForSeconds(attackHoldTime);

        // 恢复原始帧
        if (switcher != null && originalFrames != null)
        {
            switcher.frames = originalFrames;
            try { switcher.StopCycle(); } catch { }
        }
    }
}
