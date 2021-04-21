using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonVariable : MonoBehaviour
{
    public CommonVariable()
    {
        
    }
    
    public enum MoveType
    {
        // 방패가 없는 상태, 서있는 상태, 달리기, 점프, 대쉬
        EMPTY = 0, IDLE = 1, RUN = 2, JUMP = 3, DASH = 4
    }
}
