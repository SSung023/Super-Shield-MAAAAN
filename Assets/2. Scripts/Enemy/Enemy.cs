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
    [Header("Bullet")]
    public Mode bulletMode;
    protected Bullet bullet;
    [SerializeField] protected GameObject bulletGeneratePos; // bullet이 생성되는 지점
    protected int bulletCount = -1; // temp variable
    
    [HideInInspector] public int getGold;

    [Header("Variable")]
    [SerializeField] protected bool roam = true; // true일 때에만 roaming 가능
    [SerializeField] protected GameObject mother; // Hp bar, roaming point를 받기 위한 mother 오브젝트
    [HideInInspector] public int curHealth;

    
    protected int stunHealth;
    public float shield_debuff_speed = 1f;
    [SerializeField] protected EnemyData enemyData;
    
    [Header("Range")]
    [SerializeField] protected LayerMask isLayer; //OverlapBox가 적용될 Layer
    [SerializeField] protected Vector2 explosionSize; // OverlapBox의 size
    [HideInInspector] public Collider2D collider2D;
    
    
    //애니메이션 관련
    protected float scale_x;
    protected float scale_y;
    
    protected float maxStunTime;
    protected bool isPause = false;
    protected bool isDetected;

    protected bool isBeaten; // 플레이어가 방패로 총알을 막았을 때 잠시 활성화
    
    protected bool isGroggy = false;
    protected bool groggyTrigger = false;
    protected int groggyCount = 0;
    [HideInInspector] public bool isSightLeft;

    protected bool isCeiling;
    protected bool isDown;

    protected bool iscurrent;

    public void Awake()
    {
        StartCoroutine(animatorInit());
    }

    IEnumerator animatorInit()
    {
        yield return new WaitForSeconds(0.1f);
        this.gameObject.GetComponent<Animator>().enabled = false;
    }
    public void TakeHit(int damage)
    {
        int endHealth = curHealth - damage;
        
        int debuffNum = GameManager.instance.shieldManager.shield_debuff_num;
        if(debuffNum == 1)
        {
            StartCoroutine(shieldDebuffSlow());
        }
        else if (debuffNum == 2)
        {
            StartCoroutine(shieldDebuffDotDamage());
        }
        else if (debuffNum == 3)
        {
            shieldDebuffGroggy();
        }

        if (endHealth < 0)
        {
            endHealth = 0;
        }
        
        curHealth = endHealth;
        
        
        //UpdateHpBar();

        if (curHealth <= stunHealth)
        {
            if (groggyCount == 0) // 그로기 모드는 한 번만 돌입 가능
            {
                groggyTrigger = true; // 그로기 상태로 변환
                Debug.Log("그로기 상태로 전환");
            }
        }
        if(curHealth == 0)
        {
            groggyTrigger = true;
            Die();
        }
        else
        {
            StartCoroutine(TurnBeatenMode());
        }
    }
    private void dotHit(int damage)
    {
        int endHealth = curHealth - damage;

        if (endHealth < 0)
        {
            endHealth = 0;
        }
        curHealth = endHealth;
       //UpdateHpBar();
        
        if (curHealth == 0)
        {
            groggyTrigger = true;
            Die();
        }
    }

    protected void Die()
    {
        myAnimator.SetTrigger("onDeath");
        StartCoroutine(TurnDetonationMode());
    }

    protected void dropGold()
    {
        getGold = Random.Range(enemyData.MinGold, enemyData.MaxGold);
        Instantiate(enemyData.Coin, transform.position, transform.rotation);
        Debug.Log(getGold + " 드랍함");
    }

    private IEnumerator TurnBeatenMode()
    {
        isBeaten = true;
        myAnimator.SetTrigger("hit");
        yield return new WaitForSeconds(0.55f);
        isBeaten = false;
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

    protected virtual IEnumerator TurnGroggyMode(Transform _transform, float time, bool isStern)
    {
        gameObject.layer = LayerMask.NameToLayer("ShieldMaterial");
        myAnimator.SetBool("isStunned", true);
        isGroggy = true;
        
        if (isCeiling)
        {
            Vector3 tempVec = this.gameObject.transform.position;
            
            isDown = true;
            gameObject.layer = 12;
            gameObject.tag = "Interactable";

            yield return new WaitForSeconds(4.0f);
            
            isDown = false;

            for (int i = 0; i <= 50; i++)
            {
                yield return new WaitForSeconds(0.02f);
                transform.position = Vector3.Lerp(gameObject.transform.position, tempVec, 0.02f * i);
            }
            
            gameObject.layer = 10;
            gameObject.tag = "Enemy";
        }
        else
        {
            gameObject.layer = 12;
            gameObject.tag = "Interactable";

            yield return new WaitForSeconds(time);

            gameObject.layer = 10;
            gameObject.tag = "Enemy";
        }
        gameObject.layer = LayerMask.NameToLayer("Enemy");

        myAnimator.SetBool("isStunned", false);
        isGroggy = false;
        
        if (!isStern)
        {
            groggyCount++;
        }
        Debug.Log("그로기 상태에서 빠져나옴");
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
            bullet = enemyData.ShortBullet;
        }
        else if (bulletCount == 1)
        {
            bullet = enemyData.LongBullet;
        }
    }
    
    
    protected void CheckBulletMode()
    {
        switch (bulletMode)
        {
            case Mode.Long:
                bullet = enemyData.LongBullet;
                break;
            case Mode.Short:
                bullet = enemyData.ShortBullet;
                break;
        }
    }

    protected virtual void Detect()
    {
        
    }
    
    protected virtual void Attack()
    {
        
    }
    IEnumerator shieldDebuffSlow()
    {
        shield_debuff_speed = 1 - (GameManager.instance.shieldManager.level_bonus_table[GameManager.instance.shieldManager.shield_level_num] * 0.1f);
        yield return new WaitForSeconds(2.0f);
        shield_debuff_speed = 1;
    }
    IEnumerator shieldDebuffDotDamage()
    {
        for(int i = 0; i < GameManager.instance.shieldManager.level_bonus_table[GameManager.instance.shieldManager.shield_level_num]; i++)
        {
            dotHit(1);
            yield return new WaitForSeconds(0.5f);
        }
    }
    private void shieldDebuffGroggy()
    {
        float sternTime = GameManager.instance.shieldManager.level_bonus_table[GameManager.instance.shieldManager.shield_level_num] * 0.1f;
        StartCoroutine(TurnGroggyMode(transform, sternTime, true));
    }

    protected IEnumerator currentTimer(float currentTime)
    {
        yield return new WaitForSeconds(currentTime);
        iscurrent = true;
    }
    public void OnBecameVisible()
    {
        this.enabled = true;
        this.gameObject.GetComponent<Animator>().enabled = true;
    }
    public void OnBecameInvisible()
    {
        this.enabled = false;
        this.gameObject.GetComponent<Animator>().enabled = false;
    }
}
