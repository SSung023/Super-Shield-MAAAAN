using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CircleBullet : MonoBehaviour
{
    
    [HideInInspector] public CeilingEnemy mother;
    public LayerMask isLayer;
    private Rigidbody2D rigid;
    
    private RaycastHit2D raycast;

    public float speed = 5f;
    public float distance;
    public int damage = 1;

    private Vector2 shootDir;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }
    
    private void Start()
    {
        Invoke("DestroyBullet", 1f);
        
        shootDir = (GameManager.Player.transform.position - this.gameObject.transform.position).normalized;
    }
    
    
    private void Update()
    {
        Shoot();
    }

    private void Shoot()
    {
        raycast = Physics2D.CircleCast(transform.position, 0.3f, Vector2.left, 0f, isLayer);
        if (raycast.collider != null)
        {
            if (raycast.collider.tag == "Player")
            {
                Debug.Log("총 맞음");
                raycast.collider.GetComponent<Player>().TakeHit(damage);
            }
            else if(raycast.collider.tag == "Shield")
            {
                Debug.Log("방어");
                if(mother != null)
                {
                    mother.TakeHit(damage);
                }
            }
            // else if (raycast.collider.tag == "Ground")
            // {
            //     DestroyBullet();
            // }
            DestroyBullet();
        }
        else
        {
            transform.Translate(shootDir * Time.deltaTime * speed);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }

    private void DestroyBullet()
    {
        Destroy(gameObject);
    }
}
