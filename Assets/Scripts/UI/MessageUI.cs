using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// 消息提示UI - 持久化单例
/// 跨场景存在，面板由 SceneUI 动态创建并设置
/// </summary>
public class MessageUI : MonoBehaviour
{
    public static MessageUI Instance { get; private set; }

    [Header("消息面板（由 SceneUI 动态设置）")]
    public GameObject MessagePanel;
    public TextMeshProUGUI MessageText;
    
    [Header("提示面板（由 SceneUI 动态设置）")]
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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        HideMessage();
        HidePrompt();
    }

    public void ShowMessage(string message, float duration = -1)
    {
        if (_currentMessageCoroutine != null)
            StopCoroutine(_currentMessageCoroutine);

        _currentMessageCoroutine = StartCoroutine(ShowMessageCoroutine(message, duration));
    }

    IEnumerator ShowMessageCoroutine(string message, float duration)
    {
        if (MessagePanel != null) MessagePanel.SetActive(true);
        
        if (MessageText != null)
        {
            MessageText.text = "";
            foreach (char c in message)
            {
                MessageText.text += c;
                yield return new WaitForSeconds(TypingSpeed);
            }
        }

        float waitTime = duration > 0 ? duration : DefaultMessageDuration;
        yield return new WaitForSeconds(waitTime);

        HideMessage();
    }

    public void HideMessage()
    {
        if (MessagePanel != null) MessagePanel.SetActive(false);
    }

    public void ShowPrompt(string prompt)
    {
        if (PromptPanel != null)
        {
            PromptPanel.SetActive(true);
            if (PromptText != null) PromptText.text = prompt;
        }
    }

    public void HidePrompt()
    {
        if (PromptPanel != null) PromptPanel.SetActive(false);
    }

    public void ShowDialogue(string[] dialogues)
    {
        StartCoroutine(ShowDialogueCoroutine(dialogues));
    }

    IEnumerator ShowDialogueCoroutine(string[] dialogues)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetGameState(GameState.Dialogue);

        foreach (string dialogue in dialogues)
        {
            if (MessagePanel != null) MessagePanel.SetActive(true);
            if (MessageText != null)
            {
                MessageText.text = "";
                foreach (char c in dialogue)
                {
                    MessageText.text += c;
                    yield return new WaitForSeconds(TypingSpeed);
                }
            }

            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return));
            yield return null;
        }

        HideMessage();
        if (GameManager.Instance != null)
            GameManager.Instance.SetGameState(GameState.Exploration);
    }
}
