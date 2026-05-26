using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void GameStart()
    {
        SceneManager.LoadScene(1);//开始
    }
    public void ExitGame()
    {
        Application.Quit();//退出软件
    }
}
