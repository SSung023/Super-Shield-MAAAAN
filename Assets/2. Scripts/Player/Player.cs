using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Player : MonoBehaviour
{
    public int health = 5;
    public int maxHealth = 5;
    public bool isInvincible = false;

    private IEnumerator invCor;

    public bool shield_memory_barrier = false;
    public int shield_memory_maxhealth = 0;

    private Animator myAnimator;

    private void Awake()
    {
        health = maxHealth;
        invCor = GetInv(0);
        GameManager.instance.gameReset();//일단 임시로 여기에 게임 리셋을 넣어둠

        myAnimator = GetComponent<Animator>();
    }
    public void TakeHit(int damage)
    {
        var endHealth = health - damage;
        if(isInvincible) 
        {
            return;
        }
        myAnimator.SetTrigger("OnHit");
        if(shield_memory_barrier)
        {
            shield_memory_barrier = false;
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
            myAnimator.SetTrigger("OnDeath");
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

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Interactable"))
        {
            SpriteRenderer spr = other.GetComponent<SpriteRenderer>();

            Color color = spr.material.color;
            color.a = 1f;
            spr.material.color = color;
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Interactable"))
        {
            SpriteRenderer spr = other.GetComponent<SpriteRenderer>();

            Color color = spr.material.color;
            color.a = 0.6f;
            spr.material.color = color;
        }
    }
}
