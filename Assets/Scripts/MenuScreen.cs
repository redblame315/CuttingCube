using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScreen : UIScreen
{
    public override void Init()
    {
        GameManager.Instance.gameState = GameState.READY;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayButtonClicked()
    {
        UIManager.instance.mainUIScreen.Focus();
    }

    public void ShopButtonClicked()
    {
        UIManager.instance.shopUIScreen.Focus();
    }

    public void QuitButtonClicked()
    {
        Application.Quit();
    }
}
