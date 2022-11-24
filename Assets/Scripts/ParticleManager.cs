using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ParticleType
{
    FX_RED,
    FX_GREEN,
    FX_BLUE,
    FX_PURPLE
}
public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance = null;
    public GameObject[] particleArray;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayParticle(ParticleType particleType, Vector3 point)
    {
        GameObject particleObj = Instantiate(particleArray[(int)particleType]) as GameObject;
        //GameObject particleObj = Instantiate(particleArray[GMain.GetRandom(particleArray.Length)]) as GameObject;
        particleObj.transform.position = point;
        particleObj.transform.rotation = Quaternion.identity;
        particleObj.transform.localScale = Vector3.one;
    }

    public static ParticleType GetParticleType(string _name)
    {
        ParticleType particleType = ParticleType.FX_BLUE;
        if (_name.Contains("Red"))
            particleType = ParticleType.FX_RED;
        else if(_name.Contains("Green"))
            particleType = ParticleType.FX_GREEN;
        else if (_name.Contains("Blue"))
            particleType = ParticleType.FX_BLUE;
        else if (_name.Contains("Purple"))
            particleType = ParticleType.FX_PURPLE;

        return particleType;
    }
}
