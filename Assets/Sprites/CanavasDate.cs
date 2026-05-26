using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//全局不销毁
//这个脚本的作用是让挂载了这个脚本的游戏对象在场景切换时不被销毁。这样可以用来保存一些全局数据，也可以用于存读档
public class CanavasDate : MonoBehaviour
{
    CanavasDate instance;

 void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }
}