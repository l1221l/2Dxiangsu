using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerControl : MonoBehaviour
{
    [Header("�ƶ�����")]
    public float moveSpeed = 5f;

    private Animator _anim;
    private Rigidbody2D _rb;
    private Vector2 _lastDir;
    private Vector2 moveDir; // ���ƶ�������ȡ�ɳ�Ա����、
   

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _lastDir = new Vector2(0, -1);
    }

    // ��ֻ�����ȡ���롿
    void Update()
    {
        // 1. ��ȡ����
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 2. ǿ��4�����߼�
        moveDir = Vector2.zero;
        if (Mathf.Abs(h) > Mathf.Abs(v))
        {
            moveDir = new Vector2(h, 0);
        }
        else if (Mathf.Abs(v) > Mathf.Abs(h))
        {
            moveDir = new Vector2(0, v);
        }

        // 3. ���������
        if (moveDir.magnitude > 0)
        {
            _lastDir = moveDir;
        }

        // 4. ������������������ Update��
        _anim.SetFloat("Horizontal", _lastDir.x);
        _anim.SetFloat("Vertical", _lastDir.y);
        _anim.SetFloat("Speed", moveDir.magnitude);
    }

    // ��ֻ���������ƶ������� ���Ư�ơ����ߵĹؼ�
    void FixedUpdate()
    {
        if (_rb != null)
        {
            _rb.velocity = moveDir.normalized * moveSpeed;
        }
    }
}