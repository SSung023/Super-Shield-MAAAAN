using UnityEngine;
public class ShieldItem : MonoBehaviour
{
    [SerializeField] private Shield myItem;

    public Shield GetItem()
    {
        Destroy(gameObject, 0.2f);
        return myItem;
    }
}