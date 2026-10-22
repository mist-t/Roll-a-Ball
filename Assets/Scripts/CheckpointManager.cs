
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    [Header("初始复活点")]
    public Transform initialCheckpoint;

    [Header("当前复活位置")]
    public Vector3 respawnPos;

    private bool hasCheckpoint;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 优先使用 Inspector 中指定的初始存档点
        if (initialCheckpoint != null)
        {
            SetCheckpoint(initialCheckpoint.position);
        }
        else
        {
            // 没有指定初始点时，使用管理器自身的位置
            respawnPos = transform.position;
            hasCheckpoint = true;
        }
    }

    public void SetCheckpoint(Vector3 position)
    {
        respawnPos = position;
        hasCheckpoint = true;

        Debug.Log("存档点已更新，复活位置：" + respawnPos);
    }

    public bool HasCheckpoint()
    {
        return hasCheckpoint;
    }

    public void RespawnPlayer(GameObject player)
    {
        if (!hasCheckpoint)
        {
            Debug.LogError("没有有效的复活点！");
            return;
        }

        Rigidbody playerRb = player.GetComponent<Rigidbody>();

        if (playerRb != null)
        {
            playerRb.velocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }

        player.transform.position = respawnPos;

        Debug.Log("玩家已复活到：" + respawnPos);
    }
}
