using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector] public Enemy mother;
    public LayerMask isLayer;
    protected Rigidbody2D rigid;

    private bool isShooting;
    
    public float speed;
    public float distance;
    public int damage = 1;



    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }
    
    private void Start()
    {
        Invoke("DestroyBullet", 1.5f);
        isShooting = false;
    }
    
    
    private void Update()
    {
        Shoot();
    }

    private void Shoot()
    {
        RaycastHit2D raycast = Physics2D.Raycast(transform.position, transform.right * -1, distance, isLayer);
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
                raycast.collider.GetComponent<Shield>().TakeHit(damage);
                if(mother != null)
                {
                    mother.TakeHit(damage);
                }
            }
            DestroyBullet();
        }

        StartCoroutine(GoThrough());
    }

    IEnumerator GoThrough()
    {
        if (isShooting = true)
            yield return new WaitForSeconds(0.2f);
        
        isShooting = true;
        if (mother.isSightLeft)
        {
            rigid.AddForce(transform.right * -1f * speed * Time.deltaTime, ForceMode2D.Impulse);
        }
        else
        {
            rigid.AddForce(transform.right * speed * Time.deltaTime, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(0.7f);
        isShooting = false;
    }

    private void DestroyBullet()
    {
        Destroy(gameObject);
    }
}
