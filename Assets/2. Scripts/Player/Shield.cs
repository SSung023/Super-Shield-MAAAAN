using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    public int durability = 10;
    public int maxDurability = 10;

    [SerializeField] private SpriteRenderer myRenderer;
    [SerializeField] private Collider2D myCollider;
    [SerializeField] private float throwingSpeed = 5.0f;
    private bool isOnHand = true; // 플레이어의 손 안에 들려있을 때 true
    private bool isThrown = false;
    private bool isCollided = false;
    protected BoxCollider2D collider;

    public int shield_memory_durability = 0;
    public int shield_passive_throwdamege = 0;
    private void Awake()
    {
        collider = GetComponent<BoxCollider2D>();
        collider.isTrigger = true;
 
    }

    private void FixedUpdate()
    {
        if (isThrown)
        {
            transform.Translate(throwingSpeed * Time.fixedDeltaTime, 0, 0);
        }
    }
    
    public void Root()
    {
        durability = maxDurability + shield_memory_durability;
    }

    public void Throw()
    {
        isThrown = true;
        isOnHand = false;
        transform.SetParent(null);
        Destroy(gameObject, 10f);
    }

    public void TakeHit(int damage)
    {
        durability -= damage;
        if(durability < 0)
        {
            durability = 0;
        }
        if(durability == 0)
        {
            Destroy(gameObject);
        }
    }
    public void Use(bool isUse)
    {
        myRenderer.enabled = isUse;
        myCollider.enabled = isUse;
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other);
        if (other.gameObject.CompareTag("Enemy") && !isOnHand)
        {
            Debug.Log("Hit!");
            
            other.gameObject.GetComponent<Enemy>().TakeHit(durability + shield_passive_throwdamege);
            Destroy(gameObject);
        }
    }
}
