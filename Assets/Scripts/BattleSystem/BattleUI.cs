using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 战斗UI管理 - 显示玩家/敌人状态
/// </summary>
public class BattleUI : MonoBehaviour
{
    [Header("玩家UI")]
    [SerializeField] public Image playerImage;
    [SerializeField] private Text Hptext;
    [SerializeField] private Text Mptext;
    [SerializeField] private Scrollbar HpBar;
    [SerializeField] private Scrollbar MpBar;
    [SerializeField] public Transform playerTransform;

    [Header("敌人UI")]
    [SerializeField] public Image enemyImage;
    [SerializeField] private Text enemyHpText;
    [SerializeField] private Text enemyMpText;
    [SerializeField] private Scrollbar enemyHpBar;
    [SerializeField] private Scrollbar enemyMpBar;
    [SerializeField] public Transform enemyTransform;

    [Header("战斗信息")]
    [SerializeField] private Text battleMessageText;
    [SerializeField] private GameObject battlePanel;

    void Start()
    {
        if (battlePanel != null)
            battlePanel.SetActive(false);
    }

    /// <summary>
    /// 设置玩家HP
    /// </summary>
    public void SetHp(int currentHp, int maxHp)
    {
        if (Hptext != null)
            Hptext.text = $"HP: {currentHp}/{maxHp}";
        if (HpBar != null)
            HpBar.size = (float)currentHp / maxHp;
    }

    /// <summary>
    /// 设置玩家MP
    /// </summary>
    public void SetMp(int currentMp, int maxMp)
    {
        if (Mptext != null)
            Mptext.text = $"MP: {currentMp}/{maxMp}";
        if (MpBar != null)
            MpBar.size = (float)currentMp / maxMp;
    }

    /// <summary>
    /// 设置敌人HP
    /// </summary>
    public void SetEnemyHp(int currentHp, int maxHp)
    {
        if (enemyHpText != null)
            enemyHpText.text = $"HP: {currentHp}/{maxHp}";
        if (enemyHpBar != null)
            enemyHpBar.size = (float)currentHp / maxHp;
    }

    /// <summary>
    /// 设置敌人MP
    /// </summary>
    public void SetEnemyMp(int currentMp, int maxMp)
    {
        if (enemyMpText != null)
            enemyMpText.text = $"MP: {currentMp}/{maxMp}";
        if (enemyMpBar != null)
            enemyMpBar.size = (float)currentMp / maxMp;
    }

    /// <summary>
    /// 显示战斗信息
    /// </summary>
    public void ShowMessage(string message)
    {
        if (battleMessageText != null)
        {
            battleMessageText.text = message;
            StartCoroutine(ClearMessageAfterDelay(3f));
        }
    }

    IEnumerator ClearMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (battleMessageText != null)
            battleMessageText.text = "";
    }

    /// <summary>
    /// 显示/隐藏战斗面板
    /// </summary>
    public void ShowBattlePanel(bool show)
    {
        if (battlePanel != null)
            battlePanel.SetActive(show);
    }

    /// <summary>
    /// 显示战斗面板（默认显示）
    /// </summary>
    public void ShowBattlePanel()
    {
        ShowBattlePanel(true);
    }

    /// <summary>
    /// 更新玩家HP动画（带动画缓动）
    /// </summary>
    public IEnumerator AnimateHpChange(Scrollbar bar, float from, float to, float duration = 0.5f)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bar.size = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        bar.size = to;
    }
}