using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door1 : MonoBehaviour
{
    public int targetSceneIndex = 2; // 目标场景索引
    public Vector2 playerSpawnPosition ; // 玩家在新场景中的出生位置
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Character"))
        {
            SceneManager.LoadScene(targetSceneIndex);
            PlayerControl.instance.jumpScene(playerSpawnPosition);
        }
    }
}
