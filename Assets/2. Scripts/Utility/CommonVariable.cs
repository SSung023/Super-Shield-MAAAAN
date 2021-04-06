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
        EMPTY = 0, IDLE = 1, RUN = 2, JUMP = 3, DASH = 4
    }
}
