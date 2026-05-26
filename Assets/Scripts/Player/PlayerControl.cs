using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerControl : MonoBehaviour
{
<<<<<<< HEAD:Assets/Sprites/PlayerControl.cs
    [Header("�ƶ�����")]
=======
    [Header("移动设置")]
>>>>>>> 808b526ea404ab0e0166516948f6b424b4efa05b:Assets/Scripts/Player/PlayerControl.cs
    public float moveSpeed = 5f;

    private Animator _anim;
    private Rigidbody2D _rb;
    private Vector2 _lastDir;
<<<<<<< HEAD:Assets/Sprites/PlayerControl.cs
    private Vector2 moveDir; // ���ƶ�������ȡ�ɳ�Ա����、
   
=======
    private Vector2 moveDir; // 把移动方向提取成成员变量
>>>>>>> 808b526ea404ab0e0166516948f6b424b4efa05b:Assets/Scripts/Player/PlayerControl.cs

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _lastDir = new Vector2(0, -1);
    }

<<<<<<< HEAD:Assets/Sprites/PlayerControl.cs
    // ��ֻ�����ȡ���롿
    void Update()
    {
        // 1. ��ȡ����
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 2. ǿ��4�����߼�
=======
    // 【只负责获取输入】
    void Update()
    {
        // 1. 获取输入
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 2. 强制4方向逻辑
>>>>>>> 808b526ea404ab0e0166516948f6b424b4efa05b:Assets/Scripts/Player/PlayerControl.cs
        moveDir = Vector2.zero;
        if (Mathf.Abs(h) > Mathf.Abs(v))
        {
            moveDir = new Vector2(h, 0);
        }
        else if (Mathf.Abs(v) > Mathf.Abs(h))
        {
            moveDir = new Vector2(0, v);
        }

<<<<<<< HEAD:Assets/Sprites/PlayerControl.cs
        // 3. ���������
=======
        // 3. 更新最后方向
>>>>>>> 808b526ea404ab0e0166516948f6b424b4efa05b:Assets/Scripts/Player/PlayerControl.cs
        if (moveDir.magnitude > 0)
        {
            _lastDir = moveDir;
        }

<<<<<<< HEAD:Assets/Sprites/PlayerControl.cs
        // 4. ������������������ Update��
=======
        // 4. 动画参数（可以留在 Update）
>>>>>>> 808b526ea404ab0e0166516948f6b424b4efa05b:Assets/Scripts/Player/PlayerControl.cs
        _anim.SetFloat("Horizontal", _lastDir.x);
        _anim.SetFloat("Vertical", _lastDir.y);
        _anim.SetFloat("Speed", moveDir.magnitude);
    }

<<<<<<< HEAD:Assets/Sprites/PlayerControl.cs
    // ��ֻ���������ƶ������� ���Ư�ơ����ߵĹؼ�
=======
    // 【只负责物理移动】—— 解决漂移、乱走的关键
>>>>>>> 808b526ea404ab0e0166516948f6b424b4efa05b:Assets/Scripts/Player/PlayerControl.cs
    void FixedUpdate()
    {
        if (_rb != null)
        {
            _rb.velocity = moveDir.normalized * moveSpeed;
        }
    }
}
