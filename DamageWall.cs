using System.Collections;
using UnityEngine;

public class DamageWall : MonoBehaviour
{
    public PlayerStat playerStat;
    
    public int Damage;

    private void OnTriggerStay(Collider coll)
    {
        if (coll.gameObject.CompareTag("Player"))
        {
            if (playerStat.nowHP > 0)
            {
                playerStat.playerManager.Hit();
                playerStat.nowHP -= Damage;
                print("현재 HP : " + playerStat.nowHP);
            }
            else
            {
                playerStat.playerManager.Die();
                print("전기구이 사망");
                Destroy(this);
            }
            
        }

    }
}
