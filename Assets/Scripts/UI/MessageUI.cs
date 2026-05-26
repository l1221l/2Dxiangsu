using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 消息提示UI - 单例
/// </summary>
public class MessageUI : MonoBehaviour
{
    public static MessageUI Instance { get; private set; }

    [Header("消息面板")]
    public GameObject MessagePanel;
    public TextMeshProUGUI MessageText;
    
    [Header("提示面板")]
    public GameObject PromptPanel;
    public TextMeshProUGUI PromptText;

    [Header("设置")]
    public float TypingSpeed = 0.05f;
    public float DefaultMessageDuration = 3f;

    private Coroutine _currentMessageCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        HideMessage();
        HidePrompt();
    }

    /// <summary>
    /// 显示消息（自动消失）
    /// </summary>
    public void ShowMessage(string message, float duration = -1)
    {
        if (_currentMessageCoroutine != null)
            StopCoroutine(_currentMessageCoroutine);

        _currentMessageCoroutine = StartCoroutine(ShowMessageCoroutine(message, duration));
    }

    IEnumerator ShowMessageCoroutine(string message, float duration)
    {
        MessagePanel.SetActive(true);
        
        // 打字机效果
        MessageText.text = "";
        foreach (char c in message)
        {
            MessageText.text += c;
            yield return new WaitForSeconds(TypingSpeed);
        }

        // 等待
        float waitTime = duration > 0 ? duration : DefaultMessageDuration;
        yield return new WaitForSeconds(waitTime);

        HideMessage();
    }

    /// <summary>
    /// 隐藏消息
    /// </summary>
    public void HideMessage()
    {
        MessagePanel.SetActive(false);
    }

    /// <summary>
    /// 显示提示（不会自动消失）
    /// </summary>
    public void ShowPrompt(string prompt)
    {
        PromptPanel.SetActive(true);
        PromptText.text = prompt;
    }

    /// <summary>
    /// 隐藏提示
    /// </summary>
    public void HidePrompt()
    {
        PromptPanel.SetActive(false);
    }

    /// <summary>
    /// 显示剧情文本（需要按键继续）
    /// </summary>
    public void ShowDialogue(string[] dialogues)
    {
        StartCoroutine(ShowDialogueCoroutine(dialogues));
    }

    IEnumerator ShowDialogueCoroutine(string[] dialogues)
    {
        GameManager.Instance.SetGameState(GameState.Dialogue);

        foreach (string dialogue in dialogues)
        {
            MessagePanel.SetActive(true);
            MessageText.text = "";
            
            // 打字机效果
            foreach (char c in dialogue)
            {
                MessageText.text += c;
                yield return new WaitForSeconds(TypingSpeed);
            }

            // 等待按键
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return));
            yield return null; // 防止一帧内多次触发
        }

        HideMessage();
        GameManager.Instance.SetGameState(GameState.Exploration);
    }
}
