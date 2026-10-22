
using UnityEngine;

namespace ithappy
{
    [RequireComponent(typeof(Rigidbody))]
    public class RotationScript : MonoBehaviour
    {
        public enum RotationAxis
        {
            X,
            Y,
            Z
        }

        [Header("旋转设置")]
        public RotationAxis rotationAxis = RotationAxis.Y;

        [Tooltip("旋转速度，单位为度/秒")]
        public float rotationSpeed = 50f;

        [Header("碰撞弹飞设置")]
        [Tooltip("撞击冲量倍率，数值越大，奶蛋飞得越远")]
        public float impactMultiplier = 1.5f;

        [Tooltip("额外向上的冲量")]
        public float upwardImpact = 2f;

        [Tooltip("奶蛋被撞飞后的最低水平冲量")]
        public float minimumImpact = 2f;

        private Rigidbody swingRb;

        void Awake()
        {
            swingRb = GetComponent<Rigidbody>();

            // 摆锤由脚本控制旋转，不受重力影响
            swingRb.isKinematic = true;
            swingRb.useGravity = false;

            // 让物理系统平滑处理旋转
            swingRb.interpolation = RigidbodyInterpolation.Interpolate;

            // 减少快速运动时漏检碰撞的情况
            swingRb.collisionDetectionMode =
                CollisionDetectionMode.ContinuousSpeculative;
        }

        void FixedUpdate()
        {
            float rotationValue =
                rotationSpeed * Time.fixedDeltaTime;

            Vector3 axis = Vector3.zero;

            switch (rotationAxis)
            {
                case RotationAxis.X:
                    axis = Vector3.right;
                    break;

                case RotationAxis.Y:
                    axis = Vector3.up;
                    break;

                case RotationAxis.Z:
                    axis = Vector3.forward;
                    break;
            }

            // 使用物理系统更新旋转
            Quaternion deltaRotation =
                Quaternion.AngleAxis(rotationValue, axis);

            swingRb.MoveRotation(
                swingRb.rotation * deltaRotation
            );
        }

        void OnCollisionEnter(Collision collision)
        {
            // 只对带有 Rigidbody 的动态物体施加冲量
            Rigidbody hitRb = collision.rigidbody;

            if (hitRb == null || hitRb.isKinematic)
            {
                return;
            }

            // 取得实际碰撞点
            ContactPoint contact = collision.GetContact(0);

            // 计算摆锤碰撞点的切线运动速度
            Vector3 swingVelocity = Vector3.Cross(
                swingRb.angularVelocity,
                contact.point - swingRb.worldCenterOfMass
            );

            // 根据摆锤运动方向产生冲量
            Vector3 impactDirection = swingVelocity;

            if (impactDirection.sqrMagnitude < 0.01f)
            {
                // 如果旋转速度很低，则使用接触面的法线
                impactDirection = contact.normal;
            }

            impactDirection.Normalize();

            // 保留一定的水平冲击，避免只向上弹
            Vector3 horizontalDirection = Vector3.ProjectOnPlane(
                impactDirection,
                Vector3.up
            );

            if (horizontalDirection.sqrMagnitude > 0.001f)
            {
                horizontalDirection.Normalize();
            }
            else
            {
                horizontalDirection = Vector3.zero;
            }

            Vector3 impact = horizontalDirection * minimumImpact
                           + impactDirection * impactMultiplier
                           + Vector3.up * upwardImpact;

            hitRb.AddForce(impact, ForceMode.Impulse);

            Debug.Log(
                "大摆锤撞中了 " + collision.gameObject.name
                + "，已施加弹飞冲量：" + impact,
                this
            );
        }
    }
}
