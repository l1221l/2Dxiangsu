using UnityEngine;

/// <summary>
/// 消息触发器 - 玩家进入区域时显示提示
/// </summary>
public class MessageTrigger : MonoBehaviour
{
    [Header("消息设置")]
    [TextArea(3, 5)]
    public string Message = "提示信息";
    public float DisplayDuration = 5f;
    
    [Header("触发设置")]
    public bool TriggerOnce = true; // 只触发一次
    public bool ShowAsDialogue = false; // 使用对话模式（需要按键继续）
    public string[] DialogueLines; // 多行对话

    [Header("目标提示")]
    public bool IsObjective = false; // 是否是目标提示
    public string ObjectiveTitle = "目标";

    private bool _hasTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Character"))
        {
            if (TriggerOnce && _hasTriggered)
                return;

            _hasTriggered = true;

            if (ShowAsDialogue && DialogueLines.Length > 0)
            {
                MessageUI.Instance?.ShowDialogue(DialogueLines);
            }
            else
            {
                string displayMessage = IsObjective ? $"<b>{ObjectiveTitle}</b>\n{Message}" : Message;
                MessageUI.Instance?.ShowMessage(displayMessage, DisplayDuration);
            }
        }
    }
}
