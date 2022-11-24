using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitColliderControl : MonoBehaviour
{
    public static HitColliderControl Instance = null;
    public Animator animator;
    public GameObject[] hitColliderArray;

    public bool IsSlash = false;
    public string targetTagName = "";

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

    /*public void ActivateCollider(int colliderIndex)
    {
        Debug.LogError("ActivateCollider");
        
        //hitColliderArray[colliderIndex].SetActive(true);
        string tagName = colliderIndex == 0 ? "left2right" : "right2left";
        GameObject[] targetObjArray = GameObject.FindGameObjectsWithTag(tagName);
        for(int i = 0; i < targetObjArray.Length; i++)
        {
            Vector3 targetObjPosition = targetObjArray[i].transform.position;
            float distance = transform.position.z - targetObjPosition.z;
            if (transform.position.z + .3f > targetObjPosition.z &&  distance < AIPlayer.Instance.swordLength * 3f)
            {
                GameManager.Instance.hitAvailObjectList.Remove(targetObjArray[i].GetComponent<EmitObject>());
                Destroy(targetObjArray[i]);
                
                ParticleManager.Instance.PlayParticle(ParticleManager.GetParticleType(targetObjArray[i].name), targetObjArray[i].transform.position);
                GameManager.Instance.AddCoin(1);
            }
        }
    }*/

    public void ActivateCollider(int colliderIndex)
    {
        targetTagName = colliderIndex == 0 ? "left2right" : "right2left";
        IsSlash = true;
        AIPlayer.Instance.swordTrailObj.SetActive(true);
        /*GameObject[] targetObjArray = GameObject.FindGameObjectsWithTag(tagName);
        for (int i = 0; i < targetObjArray.Length; i++)
        {
            Vector3 targetObjPosition = targetObjArray[i].transform.position;
            float distance = transform.position.z - targetObjPosition.z;
            if (transform.position.z + .3f > targetObjPosition.z && distance < AIPlayer.Instance.swordLength * 3f)
            {
                GameManager.Instance.hitAvailObjectList.Remove(targetObjArray[i].GetComponent<EmitObject>());
                Destroy(targetObjArray[i]);

                ParticleManager.Instance.PlayParticle(ParticleManager.GetParticleType(targetObjArray[i].name), targetObjArray[i].transform.position);
                GameManager.Instance.AddCoin(1);
            }
        }*/
    }

    public void DeactivateHitCollider(int colliderIndex)
    {
        animator.SetBool("Attack", false);
        IsSlash = false;
        AIPlayer.Instance.swordTrailObj.SetActive(false);
    }
}
