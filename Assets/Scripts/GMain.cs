using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo
{
    public int speed;
    public int accuracy;
    public int income;
    public int coin;
    public PlayerInfo()
    {

    }

    public void Save()
    {
        PlayerPrefs.SetInt("speed", speed);
        PlayerPrefs.SetInt("accuracy", accuracy);
        PlayerPrefs.SetInt("income", income);
        PlayerPrefs.SetInt("coin", coin);
    }

    public void Load()
    {
        speed = PlayerPrefs.GetInt("speed", 30);
        accuracy = PlayerPrefs.GetInt("accuracy", 30);
        income = PlayerPrefs.GetInt("income", 0);
        coin = PlayerPrefs.GetInt("coin", 100);
    }


    
}
public class GMain
{
    public PlayerInfo playerInfo = new PlayerInfo();

    public int level = 1;

    public GMain()
    {
        playerInfo.Load();
        this.Load();
    }
    public void Save()
    {
        PlayerPrefs.SetInt("level", level);
    }

    public void Load()
    {
        level = PlayerPrefs.GetInt("level", 1);
    }    

    public static int GetRandom(int max)
    {
        return Random.Range(0, 10000) % max;
    }
}
