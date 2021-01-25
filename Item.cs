using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public GameObject ItemPrefab;
    public PlayerStat PlayerStat;
    public EasySurvivalScripts.PlayerMovement PlayerAnim;

    float TempTime;
    public float Speed;
    public int IncreaseAttackDamage = 5;

    void Start()
    {
        TempTime = Time.time;
    }

    void Update()
    {
        ItemPrefab.transform.Rotate(Vector3.back, Time.deltaTime * Speed);
    }

    private void OnTriggerStay(Collider coll)
    {
        if (Input.GetKeyDown(KeyCode.E) && coll.gameObject.CompareTag("Player"))
        {
            if (TempTime <= Time.time)
            {
                TempTime = Time.time + 1; // 1초 딜레이줘서 함수 연속으로 실행안되게함
                PickItem();
            }
        }
    }

    void PickItem()
    {
        PlayerAnim.PickFloor();
        PlayerStat.attackDmg += IncreaseAttackDamage;
        Debug.LogFormat("현재 데미지 : {0}", PlayerStat.attackDmg);
        Destroy(gameObject, 0.4f);
    }
}
