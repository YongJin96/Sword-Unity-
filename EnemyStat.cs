using UnityEngine;

public class EnemyStat : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHP;
    public int nowHP;
    public int attackDmg;

    //[Header("Ragdoll Setting")]
    //public GameObject charObj;
    //public GameObject ragdollObj;
    //public Rigidbody Spine;

    void Start()
    {
        if(gameObject.CompareTag("Enemy"))
        {
            nowHP = maxHP;
        }
    }

    public void ChangeRagdoll()
    {
        //charObj.gameObject.SetActive(false);
        //ragdollObj.gameObject.SetActive(true);
    }
}
