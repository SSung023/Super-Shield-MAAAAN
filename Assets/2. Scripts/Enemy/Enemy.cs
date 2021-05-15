﻿using System.Collections;
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
    public int maxHealth;
    protected int stunHealth;
    public float shield_debuff_speed = 1f;
    
    [Header("Range")]
    [SerializeField] protected LayerMask isLayer; //OverlapBox가 적용될 Layer
    [SerializeField] protected Vector2 explosionSize; // OverlapBox의 size
    [SerializeField] protected float detectionDistance;
    [SerializeField] protected float atkDistance;
    [HideInInspector] public Collider2D collider2D;
    protected GameObject hpBarMother;
    

    protected float currentTime;
    protected float maxStunTime;
    protected bool isPause = false; // 
    protected bool isDetected;
    protected bool isGroggy = false;
    protected bool groggyTrigger = false;
    protected int groggyCount = 0;
    [HideInInspector] public bool isSightLeft;

    protected bool isCeiling;
    protected bool isDown;

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
        UpdateHpBar();

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
    }
    private void dotHit(int damage)
    {
        int endHealth = curHealth - damage;

        if (endHealth < 0)
        {
            endHealth = 0;
        }
        curHealth = endHealth;
        UpdateHpBar();
        
        if (curHealth == 0)
        {
            groggyTrigger = true;
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

    protected virtual IEnumerator TurnGroggyMode(Transform _transform, float time, bool isStern)
    {
        _transform.GetComponent<SpriteRenderer>().color = Color.yellow;
        gameObject.layer = LayerMask.NameToLayer("ShieldMaterial");
        isGroggy = true;
        if (isCeiling)
        {
            Vector3 tempVec = this.gameObject.transform.position;
            print(tempVec);

            this.gameObject.GetComponent<Rigidbody2D>().gravityScale = 1;
            this.gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
            isDown = true;
            gameObject.layer = 12;

            yield return new WaitForSeconds(4.0f);

            
            this.gameObject.GetComponent<Rigidbody2D>().gravityScale = 0;
            isDown = false;

            for (int i = 0; i <= 50; i++)
            {
                yield return new WaitForSeconds(0.02f);
                transform.position = Vector3.Lerp(gameObject.transform.position, tempVec, 0.02f * i);
            }
            
            this.gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
            gameObject.layer = 10;
        }
        else
        {
            yield return new WaitForSeconds(time);
        }
        gameObject.layer = LayerMask.NameToLayer("Enemy");

        isGroggy = false;
        if (!isStern)
        {
            groggyCount++;
        }
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

}
