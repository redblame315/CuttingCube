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

    public Text speedCoinText;
    public Text accuracyCoinText;
    public Text incomeCoinText;

    public int speedPrice = 100;
    public int accuracyPrice = 200;
    public int incomePrice = 500;

    public int levelStepPrice = 50;

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
        int price = int.Parse(speedCoinText.text);
        if (playerInfo.coin < price)
            return;

        playerInfo.coin -= price;
        playerInfo.speed++;
        playerInfo.Save();

        UpdatePlayerInfoUI();
    }

    public void AccuracyButtonClicked()
    {
        int price = int.Parse(accuracyCoinText.text);
        if (playerInfo.coin < price)
            return;

        playerInfo.coin -= accuracyPrice;
        playerInfo.accuracy++;
        playerInfo.Save();

        UpdatePlayerInfoUI();
    }
      
    public void IncomeButtonClicked()
    {
        int price = int.Parse(incomeCoinText.text);
        if (playerInfo.coin < price)
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

        speedCoinText.text = (speedPrice + (playerInfo.speed - 1) * levelStepPrice).ToString();
        accuracyCoinText.text = (accuracyPrice + (playerInfo.accuracy - 1) * levelStepPrice).ToString();
        incomeCoinText.text = (incomePrice + (playerInfo.income - 1) * levelStepPrice).ToString();
        AIPlayer.Instance.ResetSkillValue();
    }

    public void PlayButtonClicked()
    {
        UIManager.instance.mainUIScreen.Focus();
    }
}
