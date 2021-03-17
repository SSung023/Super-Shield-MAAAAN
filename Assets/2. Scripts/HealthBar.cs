using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public GameObject mother;
    private Enemy_temp enemy;
    [SerializeField] private Image hpBar;
    private Transform pos;

    
    private void Start()
    {
        pos = mother.transform.GetChild(2);
        InitHpBarSize();
    }
    
    private void Update()
    {
        LocateBar();
        //UpdateBar();
    }
    
    private void InitHpBarSize()
    {
        hpBar.rectTransform.localScale = new Vector3(1f, 1f, 1f);
    }

    private void LocateBar()
    {
        gameObject.transform.position = new Vector3(pos.position.x + 6.3f, pos.position.y + 1.79f, pos.position.z);
    }

    public void UpdateBar()
    {
        hpBar.rectTransform.localScale = new Vector3((1f /(float)enemy.maxHealth) * (float)enemy.curHealth, 1f, 1f);
    }
}
