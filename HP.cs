using UnityEngine;

public class HP : MonoBehaviour
{
    public GameObject ItemPrefab;
    public PlayerStat PlayerStat;

    public int InCreaseHP = 25;

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.CompareTag("Player"))
        {
            PlayerStat.nowHP += InCreaseHP;
            print("현재 HP : " + PlayerStat.nowHP);
            Destroy(gameObject);
        }
    }
}
