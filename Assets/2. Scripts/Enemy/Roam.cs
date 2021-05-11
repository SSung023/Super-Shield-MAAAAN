using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roam : Enemy
{
    [SerializeField] protected Transform[] roamingPoint;
    protected int nextRoamingIndex = 0;
    
    protected bool isRoaming;
    protected bool isReached;
    
    
    protected void Roaming()
    {
        // 특정 지점을 배회하게 하는 코드
        if (!isRoaming)
        {
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
        float remainDistance = Vector2.Distance(transform.position, point);
        
        if (point.x - transform.position.x < -0.01)
        {
            isSightLeft = true;
            transform.Translate(Vector3.left * speed * Time.deltaTime * shield_debuff_speed);
            myAnimator.SetFloat("Direction", 0);
        }
        else if(point.x - transform.position.x > 0.01)
        {
            isSightLeft = false;
            transform.Translate(Vector3.right * speed * Time.deltaTime * shield_debuff_speed);
            myAnimator.SetFloat("Direction", 1);
        }
        else if (-0.01 <= remainDistance && remainDistance <= 0.01) // 해당 포인트에 도착 했다면 다음 포인트로 움직이게 한다
        {
            isReached = true;
        }

        isRoaming = false;
    }
}
