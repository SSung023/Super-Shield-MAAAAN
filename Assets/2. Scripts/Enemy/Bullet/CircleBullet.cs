using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CircleBullet : MonoBehaviour
{
    
    [HideInInspector] public CeilingEnemy mother;
    public LayerMask isLayer;
    private Rigidbody2D rigid;
    private Vector3 targetPoint;
    private Vector3 shootDirection;
    
    private RaycastHit2D raycast;

    public float speed = 5f;
    public float distance;
    public int damage = 1;



    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }
    
    private void Start()
    {
        Invoke("DestroyBullet", 1f);
        
        targetPoint = new Vector3(mother.collider2D.gameObject.transform.position.x,
            mother.collider2D.gameObject.transform.position.y - 0.8f);

        shootDirection = (targetPoint - mother.bulletGeneratePos.position).normalized;
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
            else if (raycast.collider.tag == "Ground")
            {
                DestroyBullet();
            }
            DestroyBullet();
        }
        else
        {
            //계속 이동 시키다가 땅을 만나면 destroy
            transform.Translate(shootDirection * Time.deltaTime * speed);
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
