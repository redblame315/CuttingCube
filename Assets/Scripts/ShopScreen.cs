using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopScreen : UIScreen
{
    public Text speedText;
    public Text accuracyText;
    public Text incomeText;
    public Text coinText;

    public int speedPrice = 100;
    public int accuracyPrice = 200;
    public int incomePrice = 500;

    PlayerInfo playerInfo;
    public override void Init()
    {
        GameManager.Instance.gameState = GameState.READY;
        InitShopUI();
    }

    public void InitShopUI()
    {
        playerInfo = Singleton<GMain>.Instance.playerInfo;
        UpdatePlayerInfoUI();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpeedButtonClicked()
    {
        if (playerInfo.coin < speedPrice)
            return;

        playerInfo.coin -= speedPrice;
        playerInfo.speed++;
        playerInfo.Save();

        UpdatePlayerInfoUI();
    }

    public void AccuracyButtonClicked()
    {
        if (playerInfo.coin < accuracyPrice)
            return;

        playerInfo.coin -= accuracyPrice;
        playerInfo.accuracy++;
        playerInfo.Save();

        UpdatePlayerInfoUI();
    }
      
    public void IncomeButtonClicked()
    {
        if (playerInfo.coin < incomePrice)
            return;

        playerInfo.coin -= incomePrice;
        playerInfo.income++;
        playerInfo.Save();

        UpdatePlayerInfoUI();
    }

    public void UpdatePlayerInfoUI()
    {
        speedText.text = playerInfo.speed.ToString();
        accuracyText.text = playerInfo.accuracy.ToString();
        incomeText.text = playerInfo.income.ToString();
        coinText.text = playerInfo.coin.ToString();

        AIPlayer.Instance.ResetSkillValue();
    }

    public void PlayButtonClicked()
    {
        UIManager.instance.mainUIScreen.Focus();
    }
}
