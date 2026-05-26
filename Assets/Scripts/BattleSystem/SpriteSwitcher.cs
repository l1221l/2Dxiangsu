using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 挂载到角色/敌人 Image 上，每帧切换精灵图实现闪烁动画效果。
/// 支持两种模式：
///   - Cycle：按顺序循环切换 frames 数组中的精灵
///   - Blink：在原始精灵与 blinkSprite 之间交替（受击闪烁用）
/// </summary>
public class SpriteSwitcher : MonoBehaviour
{
    [Header("循环模式")]
    public Sprite[] frames;                 // 要循环的精灵数组
    public float framesPerSecond = 8f;      // 每秒切换帧数

    [Header("闪烁模式")]
    public Sprite blinkSprite;              // 闪烁时切换到的精灵（如白色版本）
    public float blinkDuration = 0.5f;      // 闪烁总时长
    public float blinkInterval = 0.08f;     // 每次闪烁间隔

    [Header("技能动画")]
    public Sprite[] skillFrames;            // 技能释放动画帧（可选）
    public float skillFrameRate = 10f;      // 技能动画帧率

    private Image image;
    private Sprite originalSprite;
    private int frameIndex;
    private float timer;

    private bool isBlinking;
    private float blinkTimer;
    private float blinkSwapTimer;
    private bool blinkState;

    private bool isPlayingSkill;

    void Awake()
    {
        image = GetComponent<Image>();
        if (image != null)
            originalSprite = image.sprite;
    }

    void Update()
    {
        if (isPlayingSkill)
            return;

        if (isBlinking)
        {
            UpdateBlink();
        }
        else if (frames != null && frames.Length > 0)
        {
            UpdateCycle();
        }
    }

    void UpdateCycle()
    {
        timer += Time.deltaTime;
        float interval = 1f / Mathf.Max(framesPerSecond, 0.01f);

        if (timer >= interval)
        {
            timer -= interval;
            frameIndex = (frameIndex + 1) % frames.Length;
            if (image != null)
                image.sprite = frames[frameIndex];
        }
    }

    void UpdateBlink()
    {
        blinkTimer += Time.deltaTime;
        blinkSwapTimer += Time.deltaTime;

        if (blinkTimer >= blinkDuration)
        {
            // 闪烁结束，恢复原始精灵
            isBlinking = false;
            if (image != null)
                image.sprite = originalSprite;
            return;
        }

        if (blinkSwapTimer >= blinkInterval)
        {
            blinkSwapTimer -= blinkInterval;
            blinkState = !blinkState;
            if (image != null)
                image.sprite = blinkState ? (blinkSprite != null ? blinkSprite : originalSprite) : originalSprite;
        }
    }

    public void StartCycle()
    {
        frameIndex = 0;
        timer = 0f;
    }

    public void StopCycle()
    {
        frameIndex = 0;
        timer = 0f;
        if (image != null && originalSprite != null)
            image.sprite = originalSprite;
    }

    /// <summary>
    /// 触发受击闪烁
    /// </summary>
    public void TriggerBlink()
    {
        isBlinking = true;
        blinkTimer = 0f;
        blinkSwapTimer = 0f;
        blinkState = false;
        if (image != null && originalSprite == null)
            originalSprite = image.sprite;
    }

    /// <summary>
    /// 设置循环帧数组
    /// </summary>
    public void SetFrames(Sprite[] newFrames)
    {
        frames = newFrames;
        frameIndex = 0;
        timer = 0f;
    }

    /// <summary>
    /// 播放技能动画精灵帧，播放完毕后自动恢复原始精灵
    /// </summary>
    public void PlaySkillAnimation()
    {
        if (skillFrames == null || skillFrames.Length == 0)
            return;

        StopAllCoroutines();
        StartCoroutine(PlaySkillAnimCoroutine());
    }

    private System.Collections.IEnumerator PlaySkillAnimCoroutine()
    {
        isPlayingSkill = true;

        for (int i = 0; i < skillFrames.Length; i++)
        {
            if (image != null)
                image.sprite = skillFrames[i];
            yield return new WaitForSeconds(1f / Mathf.Max(skillFrameRate, 0.01f));
        }

        // 恢复原始精灵
        if (image != null && originalSprite != null)
            image.sprite = originalSprite;

        isPlayingSkill = false;
    }
}
