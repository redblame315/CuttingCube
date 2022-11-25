using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUIScreen : UIScreen
{
    public static MainUIScreen Instance = null;
    public Text coinText;
    RectTransform canvasRectTransform;
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

        canvasRectTransform = UIManager.instance.GetComponent<RectTransform>();
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

    public void ShowHitPoint(Vector3 worldPoint)
    {
        GameObject hitMessageObj = Instantiate(Resources.Load("Prefabs/HitMessage")) as GameObject;
        hitMessageObj.transform.parent = UIManager.instance.transform;
        Text hitMessageText = hitMessageObj.GetComponent<Text>();
        hitMessageText.text = "+" + GameManager.Instance.hitPoint.ToString() + "$";
        RectTransform hitMessageTransform = hitMessageObj.GetComponent<RectTransform>();

        Vector2 viewPortPosition = Camera.main.WorldToViewportPoint(worldPoint);
        Vector2 screenPosition = new Vector2(viewPortPosition.x * canvasRectTransform.sizeDelta.x - canvasRectTransform.sizeDelta.x * 0.5f,
            viewPortPosition.y * canvasRectTransform.sizeDelta.y - canvasRectTransform.sizeDelta.y * 0.5f);

        hitMessageTransform.anchoredPosition = screenPosition;
    }
}
