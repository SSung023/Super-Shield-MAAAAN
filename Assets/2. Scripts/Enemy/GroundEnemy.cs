﻿using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GroundEnemy : Enemy
{
    [SerializeField] private Transform[] roamingPoint;

    private float maxStayTime;
    private float curTime;

    private bool isRoaming;
    private bool isReached;

    private int nextRoamingIndex = 0;

    [Header("Sound")]
    [SerializeField] private AudioSource enemyAudioSource;

    private void Awake()
    {
        hpBarMother = mother.transform.GetChild(1).gameObject;
        bulletGeneratePos = transform.GetChild(0);
    }

    private void Start()
    {
        isDetected = false;
        isRoaming = false;
        isReached = false;
        isGroggy = false;
        isPause = false;

        bulletCoolTime = 1f;

        maxStunTime = 2f;
        stunHealth = 2;
        maxHealth = 5;
        curHealth = maxHealth;

        detectionBoxSize.x = detectionDistance;
    }

    private void Update()
    {
        // 쉴드 상태가 아니고 일시정지 모드가 아닌 경우
        if (!isShield && !isPause)
        {
            Detect(); // 플레이어가 범위 안에 있는지 확인
            CheckBulletMode();
            if (isGroggy)
            {
                StartCoroutine(TurnGroggyMode(transform));
            }
            else
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
        collider2D = Physics2D.OverlapBox(detectionPos.position, detectionBoxSize, 0, isLayer);
        
        if (collider2D != null)
        {
            isDetected = true;
        }
        else
        {
            isDetected = false;
        }
    }

    protected override void Attack()
    {
        // 만일 atkDistance 안에 들어오면 공격
        if (Vector2.Distance(transform.position, collider2D.transform.position) < atkDistance)
        {
            if (currentTime <= 0)
            {
                switchBullet();
                SoundManager._snd.SfxCall(enemyAudioSource,18); // 임시, 적이 총알 발사할 때 소리 재생
                Bullet bulletCopy = Instantiate(bullet, bulletGeneratePos.position, transform.rotation);
                bulletCopy.mother = this;
                currentTime = bulletCoolTime;
            }
        }
        // atkDistance 밖에 있다면 플레이어에게 접근
        else
        {
            Vector3 vector3 = new Vector3(collider2D.transform.position.x, transform.position.y);
            transform.position = Vector3.MoveTowards(transform.position, vector3, Time.deltaTime * speed * shield_debuff_speed);
        }
        currentTime -= Time.deltaTime;
    }
    
    private void Roaming()
    {
        // 특정 지점을 배회하게 하는 코드
        if (!isRoaming)
        {
            isRoaming = true;

            if (isReached)
            {
                nextRoamingIndex++;
                if (nextRoamingIndex == roamingPoint.Length)
                {
                    nextRoamingIndex = 0;
                }
                isReached = false;
            }
            Move(roamingPoint[nextRoamingIndex].position);
        }
    }

    private void Move(Vector3 point)
    {
        // point지점으로 이동하게하는 코드
        float remainDistance = Vector2.Distance(transform.position, point);
        
        if (point.x - transform.position.x < -0.01)
        {
            isSightLeft = true;
            transform.Translate(Vector3.left * speed * Time.deltaTime * shield_debuff_speed);
            myAnimator.SetFloat("Direction", 0);
        }
        else if(point.x - transform.position.x > 0.01)
        {
            isSightLeft = false;
            transform.Translate(Vector3.right * speed * Time.deltaTime * shield_debuff_speed);
            myAnimator.SetFloat("Direction", 1);
        }
        else if (-0.01 <= remainDistance && remainDistance <= 0.01) // 해당 포인트에 도착 했다면 다음 포인트로 움직이게 한다
        {
            isReached = true;
        }

        isRoaming = false;
    }
    
    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(detectionPos.position, detectionBoxSize);

        Gizmos.DrawWireCube(transform.position, explosionSize);

        if (isSightLeft)
        {
            Debug.DrawRay(transform.position, Vector3.left * detectionDistance, Color.blue);
            Debug.DrawRay(transform.position, Vector3.left * atkDistance, Color.red);
        }
        else
        {
            Debug.DrawRay(transform.position, Vector3.right * detectionDistance, Color.blue);
            Debug.DrawRay(transform.position, Vector3.right * atkDistance, Color.red);
        }
        
    }
}
