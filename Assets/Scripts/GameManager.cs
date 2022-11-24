using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Singleton<T> where T : class , new()
{
    private Singleton() { }
    private static readonly Lazy<T> instance = new Lazy<T>(() => new T());
    public static T Instance { get { return instance.Value;  } }
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;
    public float endDetectPointZ;
    public int hitPoint = 20;
    public GameState gameState = GameState.READY;
    //[HideInInspector]
    public List<EmitObject> hitAvailObjectList = new List<EmitObject>();
    [HideInInspector]
    public PlayerInfo playerInfo;
    GMain gMain;

    
    private void Awake()
    {
        Instance = this;
        gMain = Singleton<GMain>.Instance;
        playerInfo = gMain.playerInfo;

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddCoin(int _addCoin)
    {
        playerInfo.coin += _addCoin;
        playerInfo.Save();

        //GUI Show
        MainUIScreen.Instance.UpdatePlayerInfoUI();
    }
}
