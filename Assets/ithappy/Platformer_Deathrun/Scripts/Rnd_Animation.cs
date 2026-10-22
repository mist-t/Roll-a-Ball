using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ithappy
{
    public class Rnd_Animation : MonoBehaviour
    {
        [SerializeField] string titleAnim;
        [Header("弹跳力度，10~18之间调")]
        public float bouncePower = 14f;
        Animator anim;

        void Start()
        {
            anim = GetComponent<Animator>();
        }

        // 真实碰撞，碰到表面立刻触发
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Player"))
            {
                Rigidbody rb = collision.collider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // 垂直向上弹，保留奶蛋原本水平方向速度
                    rb.velocity = new Vector3(rb.velocity.x, bouncePower, rb.velocity.z);

                    // 重置二段跳，和你原来BallController的ResetJump配套
                    BallController bc = collision.collider.GetComponent<BallController>();
                    if (bc != null)
                    {
                        bc.ResetJump();
                    }

                    if (anim != null)
                    {
                        anim.SetTrigger(titleAnim);
                    }
                }
            }
        }
    }
}
