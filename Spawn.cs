using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public GameObject HP_Item;
    public GameObject Power_Item;
    public GameObject Teleport_Item;

    Vector3 SavePos;
    int nRand;

    void Update()
    {
        nRand = Random.Range(0, 100);
    }

    // 애니메이션 이벤트 함수
    void SpawnHPItem()
    {
        if (nRand < 20)
        {
            SavePos = transform.position;
            SavePos.y = .2f;
            Instantiate(HP_Item, SavePos, transform.rotation);
        }
    }

    void SpawnPowerItem()
    {
        if (nRand < 10)
        {
            SavePos = transform.position;
            SavePos.y = .2f;
            Instantiate(Power_Item, SavePos, transform.rotation);
        }
    }

    void SpawnTeleportItem() // 보스방 탈출용
    {
        SavePos = transform.position;
        SavePos.y = 1f;
        Instantiate(Teleport_Item, SavePos, transform.rotation);
    }
}
