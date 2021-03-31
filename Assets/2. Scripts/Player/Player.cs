using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Player : MonoBehaviour
{
    public int health = 5;
    public int maxHealth = 5;
    public bool isInvincible = false;

    private IEnumerator invCor;
    
    private void Awake()
    {
        health = maxHealth;
        invCor = GetInv(0);
        GameManager.instance.gameReset();//일단 임시로 여기에 게임 리셋을 넣어둠
    }
    public void TakeHit(int damage)
    {
        var endHealth = health - damage;
        if(isInvincible) 
        {
            return;
        }
        else if(endHealth < 0)
        {
            endHealth = 0;
        }
        health = endHealth;
        if(health == 0)
        {
            GetComponent<SpriteRenderer>().color = Color.black;
            //Time.timeScale = 0;
            GameManager.instance.setGameState(GameManager.State.gameover);
        }
    }
    public void GetInvincibility(float duration)
    {
        StopCoroutine(invCor);
        invCor = GetInv(duration);
        StartCoroutine(invCor);
    }

    private IEnumerator GetInv(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }
}
