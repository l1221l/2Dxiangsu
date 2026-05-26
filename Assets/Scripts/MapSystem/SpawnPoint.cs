using UnityEngine;

/// <summary>
/// 玩家出生点
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    [Header("出生点设置")]
    public string SpawnPointID = "SpawnPoint_Default";
    public bool IsDefaultSpawn = false;

    void Start()
    {
        // 如果是默认出生点，或者匹配当前楼层，将玩家移动到这里
        if (IsDefaultSpawn)
        {
            SpawnPlayer();
        }
    }

    /// <summary>
    /// 将玩家生成在此点
    /// </summary>
    public void SpawnPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Character");
        if (player != null)
        {
            player.transform.position = transform.position;
            
            // 如果有Rigidbody2D，重置速度
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }

            Debug.Log($"玩家已生成到: {SpawnPointID}");
        }
    }
}
