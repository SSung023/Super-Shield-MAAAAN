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
    private bool isThrown = false;
    private bool isCollided = false;
    private void FixedUpdate()
    {
        if (isThrown)
        {
            transform.Translate(throwingSpeed * Time.fixedDeltaTime, 0, 0);
        }
    }
    
    public void Root()
    {
        durability = maxDurability;
    }

    public void Throw()
    {
        isThrown = true;
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
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Hit!");
            
            other.gameObject.GetComponent<Enemy>().TakeHit(durability);
            Destroy(gameObject);
        }
    }
}
