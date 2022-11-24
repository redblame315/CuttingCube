using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { 
    READY,
    RUNNING,
    WIN,
    LOSE
}

public enum EmitObjectType { 
    LEFT2RIGHT,
    RIGHT2LEFT
}

public enum EmitObjectPointState { NONE, Enter, Exit}
public class EmitObject : MonoBehaviour
{
    public EmitObjectType emitObjectType = EmitObjectType.LEFT2RIGHT;

    [HideInInspector]
    public float speed = 0;
    public float maxDetectPointZ;
    public float minDetectPointZ;
    //public int index = 0;

    GameManager gameManager;
    Vector3 prePosition;
    EmitObjectPointState positionState = EmitObjectPointState.NONE;
    HitColliderControl hitColliderControl;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;
        hitColliderControl = HitColliderControl.Instance;
        prePosition = transform.position;

        AIPlayer aiPlayer = AIPlayer.Instance;
        AnimEventTime animEventTime = aiPlayer.animEventTime[(int)emitObjectType];
        maxDetectPointZ = aiPlayer.transform.position.z - speed * animEventTime.activateTime;
        minDetectPointZ = aiPlayer.transform.position.z - (speed * animEventTime.activateTime + AIPlayer.Instance.swordLength * 1f);

        maxDetectPointZ -= AIPlayer.Instance.swordLength * 0.2f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += transform.forward * speed * Time.fixedDeltaTime;

        /*if(Physics.Linecast(prePosition, transform.position))
        {
            //Particle Show
            ParticleManager.Instance.PlayParticle(ParticleType.FX_CrashObject, transform.position);
            Destroy(gameObject);
            return;
        }*/

        

        float pointZ = transform.position.z;
        if (pointZ > gameManager.endDetectPointZ)
            Destroy(gameObject);
        else if (pointZ > maxDetectPointZ && positionState < EmitObjectPointState.Exit)
        {
            Debug.LogError("EmitObject Exit");
            gameManager.hitAvailObjectList.Remove(this);
            positionState = EmitObjectPointState.Exit;
        }
        else if (pointZ > minDetectPointZ && positionState < EmitObjectPointState.Enter)
        {
            Debug.LogError("EmitObject Enter");
            gameManager.hitAvailObjectList.Add(this);
            positionState = EmitObjectPointState.Enter;
        }

        prePosition = transform.position;

    }

    private void LateUpdate()
    {
        if (hitColliderControl.IsSlash && gameObject.tag == hitColliderControl.targetTagName)
        {
            Vector3 targetObjPosition = transform.position;
            Vector3 heroPosition = hitColliderControl.transform.position;
            float distance = heroPosition.z - targetObjPosition.z;
            if (heroPosition.z + .5f > targetObjPosition.z && distance < AIPlayer.Instance.swordLength * 1.5f)
            {
                GameManager.Instance.hitAvailObjectList.Remove(this);

                ParticleManager.Instance.PlayParticle(ParticleManager.GetParticleType(gameObject.name), transform.position);
                GameManager.Instance.AddCoin(gameManager.hitPoint);
                Destroy(gameObject);
            }
        }
    }
}
