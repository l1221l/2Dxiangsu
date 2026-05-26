using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;

    private Animator _anim;
    private Rigidbody2D _rb;
    private Vector2 _lastDir;
    private Vector2 moveDir; // 把移动方向提取成成员变量

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _lastDir = new Vector2(0, -1);
    }

    // 【只负责获取输入】
    void Update()
    {
        // 1. 获取输入
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 2. 强制4方向逻辑
        moveDir = Vector2.zero;
        if (Mathf.Abs(h) > Mathf.Abs(v))
        {
            moveDir = new Vector2(h, 0);
        }
        else if (Mathf.Abs(v) > Mathf.Abs(h))
        {
            moveDir = new Vector2(0, v);
        }

        // 3. 更新最后方向
        if (moveDir.magnitude > 0)
        {
            _lastDir = moveDir;
        }

        // 4. 动画参数（可以留在 Update）
        _anim.SetFloat("Horizontal", _lastDir.x);
        _anim.SetFloat("Vertical", _lastDir.y);
        _anim.SetFloat("Speed", moveDir.magnitude);
    }

    // 【只负责物理移动】—— 解决漂移、乱走的关键
    void FixedUpdate()
    {
        if (_rb != null)
        {
            _rb.velocity = moveDir.normalized * moveSpeed;
        }
    }
}