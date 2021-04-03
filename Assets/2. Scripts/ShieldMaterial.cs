using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldMaterial : MonoBehaviour
{
    [SerializeField] protected Animator myAnimator;
    [SerializeField] private bool isMaking;
    [SerializeField] public float makingTime= 3.5f;
    protected bool isShield = false;

    [SerializeField] private ShieldItem myItem;
    private float _curMakingTime = 0f;
    private void FixedUpdate()
    {
        if (!isMaking) return;
        _curMakingTime += Time.fixedDeltaTime;
        if (_curMakingTime >= makingTime)
        {
            isMaking = false;
            var tmp = Instantiate(myItem, this.transform);
            tmp.transform.SetParent(null);
            Destroy(gameObject);
        }
    }

    public void MakeShield()
    {
        myAnimator.SetTrigger("MakeShield");
        isMaking = true;
        isShield = true;
    }
}
