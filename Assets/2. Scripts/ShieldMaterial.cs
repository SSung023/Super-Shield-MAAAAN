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

            int debuff = Random.Range(0, 4); //디버프 종류
            int passive = Random.Range(0, 5); //패시브 종류
            int level = Random.Range(0, 4); //등급

            var tmp = Instantiate(myItem, this.transform);
            tmp.transform.SetParent(null);

            ShieldItem item = tmp.GetComponent<ShieldItem>();
            item.randomShieldSet(debuff, passive, level);

            Destroy(gameObject);
            print("a");
        }
    }

    public void MakeShield()
    {
       // myAnimator.SetTrigger("MakeShield");
        isMaking = true;
        isShield = true;
    }
}
