using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

public class CeilingEnemy : Roam
{
    [SerializeField] protected float explosionDistance;

    [SerializeField] private Transform m_GroundCheck;
    [SerializeField] private Collider2D[] groundColliders;
    [SerializeField] private LayerMask m_WhatIsGround;

    const float k_GroundedRadius = .2f;
   
    
    private void Start()
    {
        scale_x = transform.localScale.x;
        scale_y = transform.localScale.y;
        
        isGroggy = false;
        
        maxStunTime = 2f;
        stunHealth = 2;
        curHealth = enemyData.MaxHealth;

        isCeiling = true;
        iscurrent = true;
    }
    
    private void Update()
    {
        // 쉴드 상태가 아니고 일시정지 모드가 아닌 경우
        if (!isShield && !isPause)
        {
            Detect(); // 플레이어가 범위 안에 있는지 확인
            CheckBulletMode();
            if (groggyTrigger)
            {
                groggyTrigger = false;
                StartCoroutine(TurnGroggyMode(transform, 4.0f, false));
            }
            if(!isGroggy && !isBeaten)
            {
                if (isDetected)
                {
                    Attack();
                }
                else
                {
                    if (roam)
                    {
                        myAnimator.SetBool("isWalking", true);
                        Roaming();
                    }
                    else
                    {
                        myAnimator.SetBool("isWalking", false);
                    }
                }
            }
        }

        if(isDown)
        {
            transform.Translate(new Vector3(0, -5.0f, 0) * Time.deltaTime);
        }

        groundColliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        
        for (int i = 0; i < groundColliders.Length; i++)
        {
            if (groundColliders[i].gameObject.layer == 9)
            {
                if (isDown)
                {
                    this.gameObject.GetComponent<Rigidbody2D>().gravityScale = 0;
                    isDown = false;
                }
            }
        }
    }
   
    protected override void Detect()
    {
        // 특정 영역에 있는 Player 감지
        collider2D = Physics2D.OverlapCircle(transform.position, enemyData.DetectionDistance, isLayer);
        if (collider2D != null)
        {
            isDetected = true;
            Debug.Log("OverlapBox로 Player 감지");
        }
        else
        {
            isDetected = false;
        }
    }

    protected override void Attack()
    {
        // atkDistance 안에 있으면 bullet 생성
        if (Vector2.Distance(transform.position, collider2D.transform.position) < enemyData.AtkDistance)
        {
            if (iscurrent)
            {
                CircleBullet bulletCopy = Instantiate(enemyData.CircleBullet, bulletGeneratePos.transform.position, transform.rotation);
                bulletCopy.mother = this;
                iscurrent = false;
                StartCoroutine(currentTimer(enemyData.BulletCoolTime));
            }
        }
        else
        {
            Vector3 vector3 = new Vector3(collider2D.transform.position.x, transform.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, vector3, Time.deltaTime * enemyData.Speed * shield_debuff_speed);
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, enemyData.DetectionDistance);
        Gizmos.DrawWireSphere(transform.position, enemyData.AtkDistance);
        Gizmos.DrawWireSphere(transform.position, explosionDistance);
    }
}
