using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance = null;
    public UIScreen mainUIScreen;
    public UIScreen menuUIScreen;
    public UIScreen shopUIScreen;
    private PlayerInfo playerInfo;
    private GameManager gameManager;

    private void Awake()
    {
        instance = this;

    }
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;        
        playerInfo = Singleton<GMain>.Instance.playerInfo;

        menuUIScreen.Focus();
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }    

    public void EndStage()
    {        
        Cursor.visible = true;
    }
}
