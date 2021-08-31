using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    Player player;
    private PlayerController controller;
    private ShieldManager shieldManager;
    
    [SerializeField] private Slider slider;
    [SerializeField] private Text sliderTx;
    [SerializeField] private Text durabilityTx;
    [SerializeField] private Text[] shield_Tx = new Text[5];
    
    private void Start()
    {
        if (player == null) player = FindObjectOfType<Player>();
        if (controller == null) controller = FindObjectOfType<PlayerController>();
        if (shieldManager == null) shieldManager = GameManager.instance.shieldManager;
        
        slider.maxValue = player.maxHealth;
        slider.minValue = 0;
        slider.value = player.health;
    }

    private void Update()
    {
        Show_PlayerHealth();
        Show_ShieldDurability();
        Show_MemoryOfShield();
        
    }

    private void Show_PlayerHealth()
    {
        slider.value = player.health;
        sliderTx.text = player.health.ToString() + " / " + (player.maxHealth+player.shield_memory_maxhealth).ToString();
    }

    private void Show_ShieldDurability()
    {
        if (controller.curShield == null)
        {
            durabilityTx.text = "None";
        }
        else
        {
            durabilityTx.text = controller.curShield.durability.ToString();
        }
    }

    private void Show_MemoryOfShield()
    {
        for (int i = 0; i < shieldManager.memory_use_number_table.Length; i++)
        {
            shield_Tx[i].text = shieldManager.memory_use_number_table[i].ToString();
        }
    }
}
