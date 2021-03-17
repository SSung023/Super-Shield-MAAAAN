using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet_temp : MonoBehaviour
{
    [HideInInspector] public Enemy mother;
    [SerializeField] protected LayerMask isLayer;
    protected Rigidbody2D rigid;
    protected RaycastHit2D raycast;

    protected bool isShooting;

    protected float durationTime;
    [SerializeField] protected float speed;
    [SerializeField] protected float distance;
    [SerializeField] protected int damage = 1;

    protected void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    protected void Start()
    {
        durationTime = 1.5f;
        Invoke("DestroyBullet", durationTime);
    }

    protected void DestroyBullet()
    {
        Destroy(gameObject);
    }

    protected void Shoot()
    {
        
    }
}
