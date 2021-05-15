using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    private int Gold = 0;
    public enum State { play = 0, stop = 1, gameover = 2 };
    private State GameState = State.play;
    private int thisStage = 0;
    public ShieldManager shieldManager;
    public static GameObject Player;
    public static Transform PlayerCenter;

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

        shieldManager = new ShieldManager();
    }

    public void goldChange(int getgold)
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


    public State getGameState()
    {
        return GameState;
    }
    public void setGameState(State nowState)
    {
        GameState = nowState;
        if(GameState == State.play)
        {
            Time.timeScale = 1;
        }
        else if (GameState == State.stop)
        {
            Time.timeScale = 0;
        }
        else if(GameState == State.gameover)
        {
            gameExit();
        }
    }
    public void gameReset()
    {
        Time.timeScale = 1;
        GameState = State.play;
        thisStage = 1;
        shieldManager.shieldReset();

    }
    public void gameExit()
    {
        SceneManager.LoadScene(0);
    }

}
