using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CeilingEnemy : Enemy
{
    
    private void Awake()
    {
        hpBarMother = mother.transform.GetChild(1).gameObject;
        bulletGeneratePos = transform.GetChild(0);
        detectionPos = transform.GetChild(1);
    }
    
    private void Start()
    {
        isGroggy = false;

        bulletCoolTime = 1f;
        
        detectionDistance = 4.5f;
        atkDistance = 3f;

        maxStunTime = 2f;
        stunHealth = 2;
        maxHealth = 5;
        curHealth = maxHealth;
    }

    
    private void Update()
    {
        Detect();
        CheckBulletMode();
        if (isGroggy)
        {
            StartCoroutine(TurnGroggyMode(transform, 2.0f , false));
        }
        else
        {
            if (isDetected)
            {
                Attack();
            }
        }
    }
    
    protected override void Detect()
    {
        // 특정 영역에 있는 Player 감지
        collider2D = Physics2D.OverlapBox(detectionPos.position, detectionBoxSize, 0, isLayer);
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
        if (currentTime <= 0)
        {
            CircleBullet bulletCopy = Instantiate(circleBullet, bulletGeneratePos.position, transform.rotation);
            bulletCopy.mother = this;
            currentTime = bulletCoolTime;
        }
        // if (Vector2.Distance(transform.position, collider2D.transform.position) < detectionDistance)
        // {
        //     if (currentTime <= 0)
        //     {
        //         CircleBullet bulletCopy = Instantiate(circleBullet, bulletGeneratePos.position, transform.rotation);
        //         bulletCopy.mother = this;
        //         currentTime = bulletCoolTime;
        //     }
        // }
        currentTime -= Time.deltaTime;
    }
    
    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(detectionPos.position, detectionBoxSize);
    }
}
