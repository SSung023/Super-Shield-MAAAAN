using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public enum Mode
{
    Long,
    Short
}
public class Enemy : ShieldMaterial
{
    [Header("Range")]
    [SerializeField] protected Transform detectionPos; // OverlapBox 위치
    [SerializeField] protected Vector2 detectionBoxSize; // OverlapBox의 size
    [HideInInspector] public Collider2D collider2D;
    [SerializeField] protected LayerMask isLayer; //OverlapBox가 적용될 Layer
    [SerializeField] protected Vector2 explosionSize; // OverlapBox의 size
    
    [Header("Bullet")]
    public Mode bulletMode;
    [HideInInspector] public Transform bulletGeneratePos; // bullet이 생성되는 지점
    protected Bullet bullet;
    [SerializeField] protected Bullet shortBullet;
    [SerializeField] protected Bullet longBullet;
    [SerializeField] protected CircleBullet circleBullet;
    [SerializeField] protected float bulletCoolTime;
    protected int bulletCount = -1; // temp variable

    [Header("Gold")]
    [Range(1, 50)]
    [SerializeField] protected int minGold = 1; // 드랍할 수 있는 최대 골드의 수
    [Range(1, 50)]
    [SerializeField] protected int maxGold = 50; // 드랍할 수 있는 최대 골드의 수
    [HideInInspector] public int getGold;
    
    [Header("Variable")]
    [SerializeField] protected GameObject mother; // Hp bar, roaming point를 받기 위한 mother 오브젝트
    [SerializeField] protected GameObject coin; // hp=0일 때 드랍할 코인의 프리팹
    [SerializeField] protected Image hpBar;
    [SerializeField] protected float speed;
    [HideInInspector] public int curHealth;
    protected GameObject hpBarMother;
    public int maxHealth;
    protected int stunHealth;

    protected float detectionDistance;
    protected float atkDistance;

    protected float currentTime;
    protected float maxStunTime;

    protected bool isPause = false; // 
    protected bool isDetected;
    protected bool isGroggy = false;
    protected int groggyCount = 0;
    [HideInInspector] public bool isSightLeft;

    

    public void TakeHit(int damage)
    {
        int endHealth = curHealth - damage;

        if(endHealth < 0)
        {
            endHealth = 0;
        }
        curHealth = endHealth;
        UpdateHpBar();

        if (curHealth <= stunHealth)
        {
            if (groggyCount == 0) // 그로기 모드는 한 번만 돌입 가능
            {
                isGroggy = true; // 그로기 상태로 변환
                Debug.Log("그로기 상태로 전환");
            }
        }
        if(curHealth == 0)
        {
            isGroggy = true;
            Die();
        }
    }

    protected void Die()
    {
        StartCoroutine(TurnDetonationMode());
        hpBarMother.SetActive(false);
    }

    protected void dropGold()
    {
        getGold = Random.Range(minGold, maxGold);
        Instantiate(coin, transform.position, transform.rotation);
        Debug.Log(getGold + " 드랍함");
    }

    protected IEnumerator TurnDetonationMode()
    {
        // 일정시간동안 모든 행동 일시정지
        isPause = true;
        yield return new WaitForSeconds(maxStunTime);
        Destroy(this.gameObject);
        dropGold();

        // 죽으면서 주변에 데미지 + 골드 드랍
        Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(transform.position, explosionSize, 0);
        Debug.Log("아이고 탱크 터져유");
        if (collider2Ds != null)
        {
            // tag가 player나 enemy인 오브젝트에게 데미지를 준다
            foreach (Collider2D col in collider2Ds)
            {
                if (col.gameObject.tag == "Enemy")
                {
                    TakeHit(1);
                }
                else if (col.gameObject.tag == "Player")
                {
                    // 코드 수정 필요(최적화 필요)
                    col.gameObject.GetComponent<Player>().TakeHit(1);
                }
            }
        }
    }

    protected virtual IEnumerator TurnGroggyMode(Transform _transform)
    {
        _transform.GetComponent<SpriteRenderer>().color = Color.yellow;
        gameObject.layer = LayerMask.NameToLayer("ShieldMaterial");
        yield return new WaitForSeconds(maxStunTime);
        gameObject.layer = LayerMask.NameToLayer("Enemy");
        
        isGroggy = false;
        groggyCount++;
        
        Debug.Log("그로기 상태에서 빠져나옴");
        _transform.GetComponent<SpriteRenderer>().color = Color.white;
    }

    protected void switchBullet()
    {
        bulletCount++;
        if (bulletCount >= 2)
        {
            bulletCount = 0;
        }
        
        if (bulletCount == 0)
        {
            bullet = shortBullet;
        }
        else if (bulletCount == 1)
        {
            bullet = longBullet;
        }
    }
    
    private void UpdateHpBar()
    {
        hpBar.rectTransform.localScale = new Vector3((1f /(float)maxHealth) * (float)curHealth, 1f, 1f);
    }
    
    protected void CheckBulletMode()
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

    protected virtual void Detect()
    {
        
    }
    
    protected virtual void Attack()
    {
        
    }
    
}
