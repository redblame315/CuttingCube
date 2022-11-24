using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmitSpawn : MonoBehaviour
{
    public EmitObjectType emitObjectType = EmitObjectType.LEFT2RIGHT;
    public GameObject[] emitObjectArray;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {        
    
    }

    public void EmitObject()
    {
        GameObject emitObject = Instantiate(emitObjectArray[GMain.GetRandom(emitObjectArray.Length)]) as GameObject;
        emitObject.transform.position = transform.position;
        emitObject.transform.rotation = transform.rotation;
        emitObject.transform.localScale = Vector3.one;
        emitObject.tag = EmitManager.getTagName(emitObjectType);
        EmitObject emitObjectScript = emitObject.GetComponent<EmitObject>();
        emitObjectScript.emitObjectType = emitObjectType;
        emitObjectScript.speed = EmitManager.Instance.speed;
    }
}
