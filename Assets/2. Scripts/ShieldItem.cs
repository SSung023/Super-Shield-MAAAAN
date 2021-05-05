using UnityEngine;
public class ShieldItem : MonoBehaviour
{
    [SerializeField] private Shield myItem;
    public int shield_debuff_num;
    public int shield_passive_num;
    public int shield_level_num;

    public Shield GetItem()
    {
        Destroy(gameObject, 0.2f);
        return myItem;
    }

    public void randomShieldSet(int debuff, int passive, int level)
    {
        shield_debuff_num = debuff;
        shield_passive_num = passive;
        shield_level_num = level;

        print("Make debuff " + debuff);
        print("Make passive " + passive);
        print("Make level " + level);
    }

    
}