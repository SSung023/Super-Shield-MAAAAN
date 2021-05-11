using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CeilingEnemy : Roam
{
    [SerializeField] protected float explosionDistance;
    private void Awake()
    {
        hpBarMother = mother.transform.GetChild(1).gameObject;
        bulletGeneratePos = transform.GetChild(0);
    }
    
    private void Start()
    {
        isGroggy = false;

        bulletCoolTime = 1f;

        maxStunTime = 2f;
        stunHealth = 2;
        maxHealth = 5;
        curHealth = maxHealth;

        isCeiling = true;
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
                StartCoroutine(TurnGroggyMode(transform, 2.0f, false));
            }
            if(!isGroggy)
            {
                if (isDetected)
                {
                    Attack();
                }
                else
                {
                    Roaming();
                }
            }
        }
        else
        {
            hpBarMother.SetActive(false);
        }
    }
    protected override void Detect()
    {
        // 특정 영역에 있는 Player 감지
        collider2D = Physics2D.OverlapCircle(transform.position, detectionDistance, isLayer);
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
        if (Vector2.Distance(transform.position, collider2D.transform.position) < atkDistance)
        {
            if (currentTime <= 0)
            {
                CircleBullet bulletCopy = Instantiate(circleBullet, bulletGeneratePos.position, transform.rotation);
                bulletCopy.mother = this;
                currentTime = bulletCoolTime;
            }
        }
        else
        {
            Vector3 vector3 = new Vector3(collider2D.transform.position.x, transform.position.y);
            transform.position = Vector3.MoveTowards(transform.position, vector3, Time.deltaTime * speed * shield_debuff_speed);
        }
        currentTime -= Time.deltaTime;
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);
        Gizmos.DrawWireSphere(transform.position, atkDistance);
        Gizmos.DrawWireSphere(transform.position, explosionDistance);
    }
}
