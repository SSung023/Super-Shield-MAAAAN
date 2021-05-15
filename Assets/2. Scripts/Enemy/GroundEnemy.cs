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
