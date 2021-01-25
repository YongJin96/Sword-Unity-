using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetAim : MonoBehaviour
{
    [Header("Target Object")]
    public Transform targetPos; // 타겟의 위치
    //public SpriteRenderer targetUI; // 타겟 UI 표시

    [Header("Targeting Setting")]
    public float ColliderRadius = 1.5f; // 충돌처리 반지름
    public float targetingSpeed = 0.1f; // 타겟을 바라보는 속도 
    public string targetTag = "Enemy";  // 타겟의 태그 (Enemy인 태그를 모두 찾기위해서)

    bool isAim = false;
    int count = 0;

    void Start()
    {
        InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }

    void UpdateTarget()
    {
        GameObject[] target = GameObject.FindGameObjectsWithTag(targetTag);
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in target)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position); // 내 캐릭터와 적과의 거리를 구하기위해
        
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null && shortestDistance <= ColliderRadius)
        {
            targetPos = nearestEnemy.transform; // 제일 가까운 적 위치 값을 넣어줌
        }
        else
        {
            targetPos = null;
        }
    }

    void Update()
    {
        SwitchKey();

        if (isAim == true)
        {
            TargetAiming();
        }
    }

    void SwitchKey()
    {
        if (Input.GetMouseButtonDown(2) && count == 0)       // 마우스 가운데 버튼
        {
            isAim = true;
            count++;

            print("Target_On");
        }
        else if (Input.GetMouseButtonDown(2) && count == 1)
        {
            isAim = false;
            count = 0;

            print("Target_Off");
        }
    }

    void TargetAiming()
    {       
        if (targetPos != null)
        {
            if (Physics.CheckSphere(transform.position, ColliderRadius, 1 << LayerMask.NameToLayer("Enemy")))
            {
                //Vector3 dir = target[].transform.position - transform.position;
                //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * targetingSpeed);
                for (int i = 0; i < 50; i++)
                {
                    Vector3 dir = targetPos.transform.position - transform.position;
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * targetingSpeed);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, ColliderRadius);
    }
}
