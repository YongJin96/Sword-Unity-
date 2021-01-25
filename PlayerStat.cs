using System.Collections;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    public GameObject player;
    public BoxCollider playerColl;
    public EasySurvivalScripts.PlayerMovement playerManager;

    public int maxHP = 100;
    public int nowHP;
    public int attackDmg;

    private bool coroutineCheck = true;

    public Transform EffectPos;
    public ParticleSystem SparkParticle;

    public EnemyStat enemyStat;

    void Start()
    {
        if (gameObject.CompareTag("Player"))
            nowHP = maxHP;
    }

    public void TakeDamage()
    {
        enemyStat.nowHP -= attackDmg;
    }

    //private void OnCollisionEnter(Collision coll)
    //{
    //    if (coll.gameObject.CompareTag("Enemy"))
    //    {
    //        StartCoroutine(AttackedDelay(coll));
    //    }
    //}

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.CompareTag("EnemyAttack"))
        {
            if (coroutineCheck == true)
            {
                coroutineCheck = false;
                StartCoroutine(AttackedDelay2(coll));
            }
        }
    }

    void SparkEffect()
    {
        SparkParticle.Play();

        Instantiate(SparkParticle, EffectPos.position, EffectPos.rotation);
    }

    IEnumerator AttackedDelay(Collision coll)
    {
        if (nowHP > 0)
        {
            if (playerManager.isShield == false)
            {
                playerManager.Hit();
                nowHP -= enemyStat.attackDmg;
                print("플레이어 HP : " + nowHP);
            }
            else if (playerManager.isShield == true)
            {
                playerManager.BlockHit();
                SparkEffect();
                print("방어");
            }
        }

        if (nowHP <= 0)
        {
            playerColl.enabled = false;
            playerManager.Die();
            print("플레이어 사망");
        }

        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator AttackedDelay2(Collider coll)
    {
        if (nowHP > 0)
        {
            if (playerManager.isShield == false && playerManager.isEvasion == false)
            {
                playerManager.Hit();
                nowHP -= enemyStat.attackDmg;
                print("플레이어 HP : " + nowHP);
            }
            else if (playerManager.isShield == true)
            {
                playerManager.BlockHit();
                SparkEffect();
                print("방어");
            }
            else if (playerManager.isShield == false && playerManager.isEvasion == true)
            {
                print("회피");
            }
        }

        if (nowHP <= 0)
        {
            gameObject.SetActive(false);
            playerManager.Die();
            print("플레이어 사망");
        }

        yield return new WaitForSeconds(0.1f);

        coroutineCheck = true;
    }
}
