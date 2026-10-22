
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool activated;

    private void OnTriggerEnter(Collider other)
    {
        if (activated || !other.CompareTag("Player"))
        {
            return;
        }

        if (CheckpointManager.Instance == null)
        {
            Debug.LogError("场景中找不到 CheckpointManager！");
            return;
        }

        CheckpointManager.Instance.SetCheckpoint(
            transform.position
        );

        activated = true;

        Debug.Log("成功激活存档点：" + gameObject.name);
    }
}
