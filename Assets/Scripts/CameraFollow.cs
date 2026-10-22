
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("奶蛋目标")]
    public Transform target;

    [Header("摄像机距离")]
    [Tooltip("摄像机距离缩放：小于1为拉近，大于1为拉远")]
    [Range(0.5f, 1.5f)]
    public float zoomScale = 0.85f;

    [Tooltip("额外高度偏移：正数抬高摄像机，负数降低")]
    public float heightOffset = 1.0f;

    [Header("跟随设置")]
    [Tooltip("跟随平滑时间，数值越小跟随越紧")]
    public float smoothTime = 0.12f;

    [Tooltip("是否跟随奶蛋上下跳跃")]
    public bool followVertical = true;

    [Header("画面大小")]
    [Tooltip("摄像机视野角度，数值越小画面越放大")]
    [Range(20f, 90f)]
    public float fieldOfView = 50f;

    private Vector3 baseOffset;
    private Vector3 velocity = Vector3.zero;
    private Quaternion baseRotation;
    private Camera cam;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError(
                "CameraFollow 没有设置 Target，请将奶蛋拖入 Target 栏位！",
                this
            );

            enabled = false;
            return;
        }

        // 记录原本设置好的摄像机位置和旋转方向
        baseOffset = transform.position - target.position;
        baseRotation = transform.rotation;

        // 记录摄像机组件
        cam = GetComponent<Camera>();

        if (cam != null)
        {
            cam.fieldOfView = fieldOfView;
        }

        // 只调整距离，不改变原来的拍摄方向
        baseOffset *= zoomScale;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        // 保留原来的水平跟随方向
        Vector3 offset = baseOffset;

        // 额外抬高摄像机，不修改原来的旋转角度
        offset.y += heightOffset;

        Vector3 targetPosition = target.position;

        // 不跟随上下跳跃时，保持摄像机原来的世界高度偏移
        if (!followVertical)
        {
            targetPosition.y = 0f;
        }

        Vector3 desiredPosition = targetPosition + offset;

        // 平滑跟随奶蛋
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );

        // 保持原本设置好的拍摄方向
        transform.rotation = baseRotation;

        // 保持固定视野角度，避免动态缩放
        if (cam != null && !Mathf.Approximately(cam.fieldOfView, fieldOfView))
        {
            cam.fieldOfView = fieldOfView;
        }
    }
}
