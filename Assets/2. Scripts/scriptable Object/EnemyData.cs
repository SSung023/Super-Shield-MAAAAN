using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Data", menuName = "Scriptable Object/Enemy Data", order = int.MaxValue)]
public class EnemyData : ScriptableObject
{
    [SerializeField]
    private float bulletCoolTime;
    public float BulletCoolTime { get { return bulletCoolTime; } }

    [SerializeField]
    private int minGold;
    public int MinGold { get { return minGold; } }

    [SerializeField]
    private int maxGold;
    public int MaxGold { get { return maxGold; } }

    [SerializeField]
    private GameObject coin;
    public GameObject Coin { get { return coin; } }

    [SerializeField] 
    private Bullet shortBullet;
    public Bullet ShortBullet { get { return shortBullet; } }
    
    [SerializeField] 
    private Bullet longBullet;
    public Bullet LongBullet { get { return longBullet; } }
    
    [SerializeField] 
    private CircleBullet circleBullet;
    public CircleBullet CircleBullet { get { return circleBullet; } }

    [SerializeField]
    private float speed;
    public float Speed { get { return speed; } }

    [SerializeField]
    private int maxHealth;
    public int MaxHealth { get { return maxHealth; } }

    [SerializeField]
    private float detectionDistance;
    public float DetectionDistance { get { return detectionDistance; } }

    [SerializeField]
    private float atkDistance;
    public float AtkDistance { get { return atkDistance; } }
}
