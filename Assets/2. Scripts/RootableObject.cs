using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootableObject : MonoBehaviour
{
    public Shield myShield;
    
    public Shield Root()
    {
        Destroy(gameObject, 0.2f);
        return myShield;
    }
}
