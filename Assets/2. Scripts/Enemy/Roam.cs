using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roam : Enemy
{
    [SerializeField] protected Transform[] roamingPoint;
    protected int nextRoamingIndex = 0;

    protected bool facingLeft = false;
    protected bool isRoaming;
    protected bool isReached = true;

    protected float remainDistance;
    
    protected void Roaming()
    {
        // 특정 지점을 배회하게 하는 코드
        if (!isRoaming)
        {
            //Debug.Log("isRoaming을 true로 변환");
            isRoaming = true;

            if (isReached)
            {
                nextRoamingIndex++;
                if (nextRoamingIndex == roamingPoint.Length)
                {
                    nextRoamingIndex = 0;
                }
                isReached = false;
            }
            Move(roamingPoint[nextRoamingIndex].position);
        }
    }

    protected void Move(Vector3 point)
    {
        // point지점으로 이동하게하는 코드
        remainDistance = Vector2.Distance(transform.position, point);
        
        if (point.x - transform.position.x < -0.02)
        {
            isSightLeft = true;
            transform.Translate(Vector3.left * speed * Time.deltaTime * shield_debuff_speed);
        }
        else if(point.x - transform.position.x > 0.02)
        {
            isSightLeft = false;
            transform.Translate(Vector3.right * speed * Time.deltaTime * shield_debuff_speed);
        }
        else if (-0.02 <= remainDistance && remainDistance <= 0.02) // 해당 포인트에 도착 했다면 다음 포인트로 움직이게 한다
        {
            // -0.02 <= remainDistance <= 0.02
            isReached = true;
            Flip();
        }

        isRoaming = false;
    }

    protected void Flip()
    {
        // 오른쪽을 보고 있을 때
        if (facingLeft)
        {
            transform.localScale = new Vector3(-scale_x, scale_y);
        }
        // 왼쪽을 보고 있을 때
        else
        {
            transform.localScale = new Vector3(scale_x, scale_y);
        }

        facingLeft = !facingLeft;
    }
}
