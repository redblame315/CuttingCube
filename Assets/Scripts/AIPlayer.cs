using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimEventTime
{
    public float activateTime;
    public float deactivateTime;
}
public class AIPlayer : MonoBehaviour
{
    public static AIPlayer Instance = null;

    public GameObject[] hitColliderArray;
    public AnimEventTime[] animEventTime;
    public GameObject swordObj;
    public GameObject swordTrailObj;
    public float swordLength;

    public float reactionTImeLimit = 3f;
    public float speed = 100;
    public float accuracy = 100;
    public float reactionTime = 0;

    GameManager gameManager;
    Animator animator;
    float determineTime = 0;
    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;
        animator = gameObject.GetComponent<Animator>();
        swordLength = hitColliderArray[0].transform.localScale.z;
        speed = gameManager.playerInfo.speed;
        accuracy = gameManager.playerInfo.accuracy;
        reactionTime = reactionTImeLimit * speed / 100.0f;
        determineTime = reactionTime;
    }
    
    // Update is called once per frame
    void LateUpdate()
    {
        if (gameManager.gameState != GameState.RUNNING)
            return;

        determineTime -= Time.deltaTime;  
        bool isAttack = animator.GetBool("Attack");        
        if(!isAttack && determineTime < 0)
        {
            DetermineAttack();
        }
    }

    public void ResetSkillValue()
    {
        speed = gameManager.playerInfo.speed;
        accuracy = gameManager.playerInfo.accuracy;
        reactionTime = reactionTImeLimit * speed / 100.0f;
        determineTime = reactionTime;
    }

    public void DetermineAttack()
    {
        Debug.LogError("-----------------DetermineAttack-----------");
        List<EmitObject> hitAvailObjs = gameManager.hitAvailObjectList;
        if(hitAvailObjs.Count > 0)
        {
            int direction = (int)hitAvailObjs[0].emitObjectType;
            if(IsCorrectDecision())
                animator.SetInteger("Direction", direction);
            else
                animator.SetInteger("Direction", 1 - direction);

            animator.SetBool("Attack", true);
            float motionValue = Random.Range(0, 10000) / 10000.0f;
            animator.SetFloat("motionValue", motionValue);
            Debug.LogError("DetermineAttack : " + direction.ToString());
        }
        determineTime = reactionTime;
    }

    public bool IsCorrectDecision()
    {
        return (Random.Range(0, 10000) % 100) < accuracy;
    }
    
}
