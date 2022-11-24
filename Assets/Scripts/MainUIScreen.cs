using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUIScreen : UIScreen
{
    public static MainUIScreen Instance = null;
    public Text coinText;
    PlayerInfo playerInfo;
    ShopScreen shopScreen;

    private void Awake()
    {
        Instance = this;
    }
    public override void Init()
    {
        playerInfo = GameManager.Instance.playerInfo;
        UpdatePlayerInfoUI();
        GameManager.Instance.gameState = GameState.RUNNING;

        shopScreen = gameObject.GetComponent<ShopScreen>();
        shopScreen.InitShopUI();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdatePlayerInfoUI()
    {
        coinText.text = playerInfo.coin.ToString();
    }

    public void ShopButtonClicked()
    {
        UIManager.instance.shopUIScreen.Focus();
    }

    public void HomeButtonClicked()
    {
        UIManager.instance.menuUIScreen.Focus();
    }
}
