using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GroundEnemy : Roam
{
    [SerializeField] protected Transform detectionPos; // OverlapBox 위치
    [SerializeField] protected Vector2 detectionBoxSize; // OverlapBox의 size
    
    [Header("Sound")]
    [SerializeField] private AudioSource enemyAudioSource;

    private void Start()
    {
        scale_x = transform.localScale.x;
        scale_y = transform.localScale.y;
        
        isDetected = false;
        isRoaming = false;
        isReached = false;
        isGroggy = false;
        isPause = false;
        iscurrent = true;

        maxStunTime = 2f;
        stunHealth = 2;
        curHealth = enemyData.MaxHealth;

        detectionBoxSize.x = enemyData.DetectionDistance;

        isCeiling = false;
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
        if (Vector2.Distance(transform.position, collider2D.transform.position) < enemyData.AtkDistance)
        {
            if (iscurrent)
            {
                switchBullet();
                SoundManager._snd.SfxCall(enemyAudioSource,18); // 임시, 적이 총알 발사할 때 소리 재생
                Bullet bulletCopy = Instantiate(bullet, bulletGeneratePos.transform.position, transform.rotation);
                bulletCopy.mother = this;
                iscurrent = false;
                StartCoroutine(currentTimer(enemyData.BulletCoolTime));
                
                myAnimator.SetTrigger("attack");
            }
        }
        // atkDistance 밖에 있다면 플레이어에게 접근
        else
        {
            Vector3 vector3 = new Vector3(collider2D.transform.position.x, transform.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, vector3, Time.deltaTime * enemyData.Speed * shield_debuff_speed);
        }
    }
    
    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(detectionPos.position, detectionBoxSize);

        Gizmos.DrawWireCube(transform.position, explosionSize);

        // if (isSightLeft)
        // {
        //     Debug.DrawRay(transform.position, Vector3.left * enemyData.DetectionDistance, Color.blue);
        //     Debug.DrawRay(transform.position, Vector3.left * enemyData.AtkDistance, Color.red);
        // }
        // else
        // {
        //     Debug.DrawRay(transform.position, Vector3.right * enemyData.DetectionDistance, Color.blue);
        //     Debug.DrawRay(transform.position, Vector3.right * enemyData.AtkDistance, Color.red);
        // }
        
    }
}
