using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTeleport : MonoBehaviour
{
    [Header("目标场景名称（和构建设置里的一致）")]
    public string targetSceneName = "Inside";
    [Header("屋内出生点物体名称")]
    public string spawnPointName = "PlayerSpawnPoint";

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 只对 Player 标签的物体生效
        if (other.CompareTag("Player"))
        {
            Debug.Log("触发了传送，准备加载场景：" + targetSceneName);
            // 注册场景加载完成事件
            SceneManager.sceneLoaded += OnSceneLoaded;
            // 加载目标场景
            SceneManager.LoadScene(targetSceneName);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 取消事件注册，防止重复调用
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // 找到玩家和出生点
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject spawn = GameObject.Find(spawnPointName);

        if (player != null && spawn != null)
        {
            player.transform.position = spawn.transform.position;
            Debug.Log("传送成功，玩家已定位到出生点");
        }
        else
        {
            Debug.LogError("传送失败！玩家或出生点找不到，请检查名称和标签");
        }
    }
}