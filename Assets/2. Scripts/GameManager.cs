using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    private int Gold = 0;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        InitGame();
    }

    void InitGame()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.SetResolution(1920, 1080, true);
        Application.targetFrameRate = 60;
    }

    public void GoldChange(int getgold)
    {
        Gold += getgold;
        Debug.Log(Gold);
    }
    public int getGold()
    {
        return Gold;
    }
    public void setGold(int setGold)
    {
        Gold = setGold;
    }
}
