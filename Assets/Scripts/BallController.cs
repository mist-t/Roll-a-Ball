
using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("移动设置")]
    public float movePower = 7f;

    [Header("跳跃设置")]
    public float jumpForce = 3.2f;
    public int maxJumpCount = 2;

    [Header("地面检测设置")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Tooltip("地面检测距离，可适当增加")]
    public float groundCheckDistance = 0.15f;

    [Header("死亡与复活设置")]
    public float deathY = -1f;

    private Rigidbody rb;
    private Collider playerCollider;

    private int jumpCount = 0;
    private bool jumpRequested = false;
    private bool isRespawning = false;
    private bool wasGrounded = false;

    // 防止起跳后因为检测到地面而立刻恢复跳跃次数
    private bool hasLeftGround = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<Collider>();

        if (rb == null)
        {
            Debug.LogError("奶蛋缺少 Rigidbody 组件！", this);
            enabled = false;
            return;
        }

        if (playerCollider == null)
        {
            Debug.LogError("奶蛋缺少 Collider 组件！", this);
            enabled = false;
            return;
        }

        wasGrounded = CheckGrounded();

        Debug.Log(
            "奶蛋控制器已启动。初始地面检测：" + wasGrounded,
            this
        );
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            jumpRequested = true;
        }

        if (!isRespawning && transform.position.y < deathY)
        {
            Debug.Log("奶蛋掉出场景，准备复活！", this);
            Respawn();
        }
    }

    void FixedUpdate()
    {
        if (isRespawning)
        {
            return;
        }

        // =========================
        // 1. 移动控制
        // =========================

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDir;

        if (Camera.main != null)
        {
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;

            camForward.y = 0f;
            camRight.y = 0f;

            camForward.Normalize();
            camRight.Normalize();

            moveDir = camForward * vertical + camRight * horizontal;
        }
        else
        {
            moveDir = new Vector3(horizontal, 0f, vertical);
        }

        moveDir = Vector3.ClampMagnitude(moveDir, 1f);
        rb.AddForce(moveDir * movePower, ForceMode.Acceleration);

        // =========================
        // 2. 检查是否落地
        // =========================

        bool isGrounded = CheckGrounded();

        // 先确认奶蛋已经离开地面，之后重新接触地面才恢复次数
        if (!isGrounded)
        {
            hasLeftGround = true;
        }

        if (isGrounded &&
            hasLeftGround &&
            rb.velocity.y <= 0.1f)
        {
            if (jumpCount > 0)
            {
                jumpCount = 0;

                Debug.Log(
                    "检测到奶蛋落地，跳跃次数已恢复为 0。",
                    this
                );
            }

            hasLeftGround = false;
        }

        // =========================
        // 3. 执行跳跃
        // =========================

        if (jumpRequested)
        {
            if (jumpCount < maxJumpCount)
            {
                rb.velocity = new Vector3(
                    rb.velocity.x,
                    0f,
                    rb.velocity.z
                );

                rb.AddForce(
                    Vector3.up * jumpForce,
                    ForceMode.VelocityChange
                );

                jumpCount++;
                hasLeftGround = true;

                Debug.Log(
                    "当前跳跃次数：" +
                    jumpCount + "/" + maxJumpCount,
                    this
                );
            }
            else
            {
                Debug.Log(
                    "二段跳次数已用完，请落地后再跳跃。",
                    this
                );
            }

            jumpRequested = false;
        }

        wasGrounded = isGrounded;
    }

    // =========================
    // 4. 地面检测
    // =========================

    bool CheckGrounded()
    {
        Bounds bounds = playerCollider.bounds;

        // 从奶蛋碰撞体底部略上方开始检测
        Vector3 origin = new Vector3(
            bounds.center.x,
            bounds.min.y + 0.05f,
            bounds.center.z
        );

        float castRadius = Mathf.Max(
            0.05f,
            Mathf.Min(groundCheckRadius, bounds.extents.x * 0.8f)
        );

        bool grounded = Physics.SphereCast(
            origin,
            castRadius,
            Vector3.down,
            out RaycastHit hit,
            groundCheckDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        return grounded;
    }

    // =========================
    // 5. 死亡与复活
    // =========================

    void Respawn()
    {
        if (isRespawning)
        {
            return;
        }

        isRespawning = true;

        Vector3 respawnPosition = transform.position;

        if (CheckpointManager.Instance != null)
        {
            respawnPosition =
                CheckpointManager.Instance.respawnPos;
        }

        Vector3 safePosition =
            respawnPosition + Vector3.up * 0.5f;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = safePosition;
        transform.position = safePosition;

        jumpCount = 0;
        jumpRequested = false;
        hasLeftGround = false;
        wasGrounded = false;

        Debug.Log("奶蛋已复活，位置：" + safePosition, this);

        isRespawning = false;
    }

    // =========================
    // 6. Scene 视图调试辅助
    // =========================

    void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();

        if (col == null)
        {
            return;
        }

        Bounds bounds = col.bounds;

        Vector3 origin = new Vector3(
            bounds.center.x,
            bounds.min.y + 0.05f,
            bounds.center.z
        );

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            origin,
            Mathf.Max(0.05f, groundCheckRadius)
        );

        Gizmos.DrawLine(
            origin,
            origin + Vector3.down * groundCheckDistance
        );
    }

    //蘑菇调用，重置跳跃计数
    public void ResetJump()
    {
        jumpCount = 0;
        hasLeftGround = false;
    }

}
