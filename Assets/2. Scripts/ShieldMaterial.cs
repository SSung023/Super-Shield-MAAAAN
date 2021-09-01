using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldMaterial : MonoBehaviour
{
    [SerializeField] protected Animator myAnimator;
    [SerializeField] protected bool isMaking;
    [SerializeField] public float makingTime= 3.5f;
    protected bool isShield = false;

    [SerializeField] private ShieldItem myItem;
    private float _curMakingTime = 0f;

    private void FixedUpdate()
    {
        if (!isMaking) return;

        StopAllCoroutines();
        _curMakingTime += Time.deltaTime;
        if (_curMakingTime >= makingTime)
        {
            isMaking = false;

            int debuff = Random.Range(0, 4); //디버프 종류
            int passive = Random.Range(0, 5); //패시브 종류
            int level = Random.Range(0, 4); //등급

            var tmp = Instantiate(myItem, this.transform);
            tmp.transform.SetParent(null);
            tmp.transform.localScale = new Vector3(0.1f, 0.3f, 1);

            ShieldItem item = tmp.GetComponent<ShieldItem>();
            //item.randomShieldSet(debuff, passive, level);

            // 이 부분으로 인해 방패가 전부 없어지고 있었습니다.
            // 들어가 있는 이유를 잘 모르겠네요 ㅎㅎ;
            // if (gameObject.transform.parent != null)
            // {
            //     
            //     Destroy(gameObject.transform.parent.gameObject);
            // }
            Destroy(gameObject);
        }
    }

    public void MakeShield()
    {
        isMaking = true;
        isShield = true;
        myAnimator.SetTrigger("MakeShield");
    }
    
}
