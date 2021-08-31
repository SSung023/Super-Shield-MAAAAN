using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ShieldManager
{
    public int[] level_bonus_table = new int[] { 1, 2, 3, 4, 5 }; //등급에 따른 벨류
    public int[] memory_use_number_table = new int[] { 0, 0, 0, 0, 0 }; //방패의 기억 사용 횟수
    public int shield_debuff_num = 0; //디버프 종류
    public int shield_passive_num = 0; //패시브 종류
    public int shield_level_num = 0; //등급
    public ShieldManager()
    {
        
    }

    
    public void shieldReset()
    {
        for (int i = 0; i < memory_use_number_table.Length; i++)
        {
            memory_use_number_table[i] = 0;
            shield_debuff_num = 0;
            shield_passive_num = 0; 
            shield_level_num = 0;
        }

        // tmp code: 시작 시 들고 있는 방패의 등급 normal로 임시 설정
        memory_use_number_table[0] = 1;
    }

    public void makeShield(PlayerController controller, Player player, Shield shield, int debuff, int passive, int level)
    {
        shield_debuff_num = debuff; //디버프 종류
        shield_passive_num = passive; //패시브 종류
        shield_level_num = level; //등급
        //일단 임시 수치를 넣어둠

        //패시브 효과 적용
        if(shield_passive_num == 1)
        {
            controller.shield_passive_speed = 1.2f * level_bonus_table[shield_level_num];
        }
        else if(shield_passive_num == 2)
        {
            shield.shield_passive_throwdamege = 1 * level_bonus_table[shield_level_num];
        }
        else if (shield_passive_num == 3)
        {
            controller.controller.shield_passive_jumpbonus = 1 * level_bonus_table[shield_level_num];
        }
        else if (shield_passive_num == 4)
        {
             controller.shield_passive_dashcooldown = 1 * level_bonus_table[shield_level_num];
        }


        //방패의 기억 효과 적용
        memory_use_number_table[shield_level_num]++;

        if(memory_use_number_table[0] >= 1 && shield_level_num == 0)
        {
            shield.shield_memory_durability = 5;
        }
        else if (memory_use_number_table[1] >= 1 && shield_level_num == 1)
        {
            player.shield_memory_barrier = true;
        }
        else if (memory_use_number_table[2] >= 1 && shield_level_num == 2)
        {
            player.shield_memory_maxhealth = 5;
        }
        else if (memory_use_number_table[3] >= 1 && shield_level_num == 3)
        {
            player.shield_memory_maxhealth = 5;
            shield.shield_memory_durability = 5;
        }
        else if (memory_use_number_table[4] >= 1 && shield_level_num == 4)
        {
            player.shield_memory_barrier = true;
            player.shield_memory_maxhealth = 5;
            shield.shield_memory_durability = 5;
        }

        shield.Root();

        Debug.Log("Hold debuff: " +shield_debuff_num);
        Debug.Log("Hold passive: " + shield_passive_num);
        Debug.Log("Hold level: " + shield_level_num);




    }
    public void throwShield(PlayerController controller, Player player, Shield shield)
    {
        //패시브 효과와 방패의 기억 효과 모두 리셋
        controller.shield_passive_speed = 1f;
        shield.shield_passive_throwdamege = 0;
        controller.controller.shield_passive_jumpbonus = 0;
        controller.shield_passive_dashcooldown = 0;
        shield.shield_memory_durability = 0;
        player.shield_memory_barrier = false;
        player.shield_memory_maxhealth = 0;

        shield_debuff_num = 0;
        shield_passive_num = 0;
        shield_level_num = 0;
    }

}
