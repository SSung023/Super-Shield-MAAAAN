using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Enemy_temp : MonoBehaviour
{
    [SerializeField] protected LayerMask isLayer; // 탐색할 레이어
    [SerializeField] protected Transform[] roamingPoint; // 로밍하는 구간의 시작점, 끝점 *
    [SerializeField] protected Image hpBar;
    protected Bullet bullet;
    [SerializeField] protected Bullet shortBullet;
    [SerializeField] protected Bullet longBullet;
    public Mode bulletMode;

    [SerializeField] private float speed; // 적이 움직일 속도
    public int maxHealth = 5;
    public int curHealth;
    private int stunHealth = 2;

    private float detectionDistance; // raycast로 감지할 거리
    protected float atkDistance; // 공격 범위
    protected Transform rayOrigin;
    protected RaycastHit2D raycast;
    protected Transform bulletGeneratePos;
    protected HealthBar healthBar;
    protected GameObject enemy;

    
    //
    protected float bulletCoolTime;
    private float currentTime;

    private float maxStayTime;
    private float maxStunTime;
    private float curTime;

    private bool isRoaming = false;
    protected bool isDetected = false;
    [HideInInspector] public bool isSightLeft = true;
    private bool isGroggy = false;
    
    private int nextRoamingIndex = 0;



    private void Awake()
    {
        enemy = transform.GetChild(0).gameObject;
        
        bulletGeneratePos = enemy.transform.GetChild(0); // ??? error?
        rayOrigin = enemy.transform.GetChild(1);
        
    }
    
    private void Start()
    {
        detectionDistance = 5f;
        atkDistance = 3f;

        bulletCoolTime = 1f;
        maxStayTime = 1f;
        maxStunTime = 2f;

        curHealth = maxHealth;
    }

    
    private void Update()
    {
        Debug.Log(curHealth);
    }

    private void FixedUpdate()
    {
        DetectPlayer();
        CheckBulletMode();
        if (isGroggy)
        {
            //코루틴 실행, 2초 후 다시 원상태로 복귀
            StartCoroutine(TurnGroggyMode());
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
    
    
    /*private void Detect()
    {
        // 현재 위치에서 좌측으로 ray를 발사하여 Player 감지
        RaycastHit2D raycast = Physics2D.Raycast(rayOrigin.position, Vector2.right * -1, detectionDistance, isLayer);
        Debug.DrawRay(rayOrigin.position, Vector3.right * -1 * detectionDistance, Color.white);
        Debug.DrawRay(rayOrigin.position, Vector3.right * -1 * atkDistance, Color.red);
        
        
        // 만일 Player를 감지했다면 해당 방향으로 이동
        if (raycast.collider != null)
        {
            curTime = 0;
            if (Vector2.Distance(transform.position, raycast.collider.transform.position) < atkDistance)
            {
                if (currentTime <= 0)
                {
                    // bullet 오브젝트를, bulletGeneratePos.position의 위치에서 생성
                    Bullet bulletCopy = Instantiate(bullet, bulletGeneratePos.position, transform.rotation);;
                    bulletCopy.mother = this;
                    currentTime = bulletCoolTime;
                }
            }
            else
            {   
                // 공격 범위 밖에 있는 경우 현재 위치에서 Player의 위치로 이동
                transform.position = Vector3.MoveTowards(transform.position, raycast.collider.transform.position,
                    Time.deltaTime * speed);
            }

            currentTime -= Time.deltaTime;
        }
        
        // 만약 Player가 감지되지 않는다면
        else
        {
            curTime += Time.deltaTime;
            if (curTime >= maxStayTime)
            {
                transform.position = Vector3.MoveTowards(transform.position, startPosition, Time.deltaTime * speed);
                
            }
        }
    }*/

    IEnumerator TurnGroggyMode()
    {
        TurnToShield();
        yield return new WaitForSeconds(maxStunTime);
        isGroggy = false;
        enemy.transform.GetComponent<SpriteRenderer>().color = Color.white;
    }

    private void CheckBulletMode()
    {
        switch (bulletMode)
        {
            case Mode.Long:
                bullet = longBullet;
                break;
            case Mode.Short:
                bullet = shortBullet;
                break;
        }
    }
    
    private void Attack()
    {
        curTime = 0;
        if (Vector2.Distance(enemy.transform.position, raycast.collider.transform.position) < atkDistance)
        {
            if (currentTime <= 0)
            {
                // bullet 오브젝트를, bulletGeneratePos.position의 위치에서 생성
                Bullet bulletCopy = Instantiate(bullet, bulletGeneratePos.position, enemy.transform.rotation);
                //bulletCopy.mother = this;
                currentTime = bulletCoolTime;
            }
        }
        else
        {   
            // 공격 범위 밖에 있는 경우 현재 위치에서 Player의 위치로 이동
            enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, raycast.collider.transform.position,
                Time.deltaTime * speed);
        }

        currentTime -= Time.deltaTime;
    }

    private void DetectPlayer()
    {
        // 현재 위치에서 좌측으로 ray를 발사하여 Player 감지
        if (isSightLeft)
        {
            raycast = Physics2D.Raycast(rayOrigin.position, Vector2.right * -1, detectionDistance, isLayer);
            Debug.DrawRay(rayOrigin.position, Vector3.right * -1 * detectionDistance, Color.white);
            Debug.DrawRay(rayOrigin.position, Vector3.right * -1 * atkDistance, Color.red);
        }
        else
        {
            raycast = Physics2D.Raycast(rayOrigin.position, Vector2.right, detectionDistance, isLayer);
            Debug.DrawRay(rayOrigin.position, Vector3.right * detectionDistance, Color.white);
            Debug.DrawRay(rayOrigin.position, Vector3.right * atkDistance, Color.red);
        }

        // 만일 Player를 감지했다면
        if (raycast.collider != null)
        {
            isDetected = true;
        }
        
        // 만약 Player가 감지되지 않는다면
        else
        {
            isDetected = false;
        }
    }

    private void Roaming()
    {
        if (!isRoaming && !isDetected)
        {
            isRoaming = true;
        
            // 만일 마지막 로밍 포인트까지 도달했다면, 처음 로밍 포인트로 리셋
            if (nextRoamingIndex == roamingPoint.Length)
            {
                nextRoamingIndex = 0;
            }
            // 다음 로밍 포인트로 이동
            Move(roamingPoint[nextRoamingIndex++].position);
        }
    }

    private void Move(Vector3 point)
    {
        // point 지점으로 이동하는 코드
        float remainDistance = Vector2.Distance(enemy.transform.position, point);

        if (point.x - enemy.transform.position.x < 0)
        {
            // 왼쪽으로 가야 함
            isSightLeft = true;
            enemy.transform.localScale = new Vector3(0.39f, 0.39f, 1);

        }
        else
        {
            // 오른쪽으로 가야 함
            isSightLeft = false;
            enemy.transform.localScale = new Vector3(-0.39f, 0.39f, 1);
        }

        // DoMove가 진행 되는 동안 ray를 발사하여 해야 함
        Sequence sequence = DOTween.Sequence()
            .Append(enemy.transform.DOMove(point, remainDistance / speed).SetEase(Ease.Linear))
            .OnComplete(() => { isRoaming = false; });


    }

    private void UpdateHpBar()
    {
        hpBar.rectTransform.localScale = new Vector3((1f /(float)maxHealth) * (float)curHealth, 1f, 1f);
        
    }

    public void TakeHit(int damage)
    {
        int endHealth = curHealth - damage;

        if(endHealth < 0)
        {
            endHealth = 0;
        }
        curHealth = endHealth;
        UpdateHpBar();
        //healthBar.UpdateBar();

        if (curHealth <= stunHealth)
        {
            isGroggy = true; // 그로기 상태로 변환
        }
        if(curHealth == 0)
        {
            Destroy(this.gameObject);
        }
    }

    private void TurnToShield()
    {
        // 일단 색만 변하는 걸로
        enemy.transform.GetComponent<SpriteRenderer>().color = Color.yellow;
        Debug.Log("그로기 상태로 전환\n");
    }
}
