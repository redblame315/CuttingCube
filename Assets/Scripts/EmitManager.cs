using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmitManager : MonoBehaviour
{
    public static EmitManager Instance = null;
    public float interval = 1f;
    public float speed = 10f;

    float curTime;
    EmitSpawn[] emitSpawnArray;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        emitSpawnArray = gameObject.GetComponentsInChildren<EmitSpawn>();
        curTime = interval;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.gameState != GameState.RUNNING)
            return;

        curTime -= Time.deltaTime;
        if(curTime < 0)
        {
            EmitSpawn emitSpawn = emitSpawnArray[GMain.GetRandom(emitSpawnArray.Length)];
            emitSpawn.EmitObject();
            curTime = interval;
        }
    }

    public static string getTagName(EmitObjectType emitObjectType)
    {
        string tagName = "left2right";
        switch(emitObjectType)
        {
            case EmitObjectType.LEFT2RIGHT:
                tagName = "left2right";
                break;
            case EmitObjectType.RIGHT2LEFT:
                tagName = "right2left";
                break;
        }

        return tagName;
    }
}
