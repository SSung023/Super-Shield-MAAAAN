using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    Player player;
    private PlayerController controller;
    [SerializeField] private Slider slider;
    [SerializeField] private Text sliderTx;
    [SerializeField] private Text durabilityTx;
    
    void Start()
    {
        if (player == null) player = FindObjectOfType<Player>();
        if(controller == null) controller = FindObjectOfType<PlayerController>();
        slider.maxValue = player.maxHealth;
        slider.minValue = 0;
        slider.value = player.health;
    }

    void Update()
    {
        slider.value = player.health;
        sliderTx.text = player.health.ToString() + " / " + (player.maxHealth+player.shield_memory_maxhealth).ToString();
        if (controller.curShield == null)
        {
            durabilityTx.text = "None";
        }
        else
        {
            durabilityTx.text = controller.curShield.durability.ToString();
        }
        
    }
}
