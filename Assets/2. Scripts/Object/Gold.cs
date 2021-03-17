using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gold : MonoBehaviour
{
    private RaycastHit2D raycast;
    [SerializeField] private LayerMask isLayer; // ground, player 레이어 체크
    
    [SerializeField] private float radius; // circle cast의 반지름

    
    
    
    private void Start()
    {
        
    }

    private void Update()
    {
        checkLayer();
    }

    private void checkLayer()
    {
        raycast = Physics2D.CircleCast(transform.position, radius, Vector2.down, 0f, isLayer);

        if (raycast.collider != null)
        {
            if (raycast.collider.tag == "Player")
            {
                // 일단은 그냥 사라지게 구현
                Destroy(this.gameObject);
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 0.65f);
    }
    
}
