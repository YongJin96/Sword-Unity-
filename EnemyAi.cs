using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAi : MonoBehaviour
{
    public enum CurrentState
    {
        idle,
        trace,
        attack,
        dead,
        hit,
        block,
        finish,
        kick
    };

    [Header("Enemy States")]
    public CurrentState curState = CurrentState.idle;

    [Header("Colliders")]
    public BoxCollider coll;         // Hand Attack Collider
    public BoxCollider legColl;      // Kick Attack Collider
    public MeshCollider WeaponColl;  // 무기 판정 콜라이더

    private new Transform transform;
    [HideInInspector]
    public Transform playerTransform;
    private NavMeshAgent nvAgent;
    private Animator animator;

    [Header("Trails")]

    [Header("Hit Particles")]
    public ParticleSystem hitParticle;
    public ParticleSystem hitParticle2;
    public ParticleSystem hitParticle3;

    public Transform playerBlade;

    [Header("Enemy Settings")]
    // 추적 사정거리
    public float traceDist = 15f;

    // 공격 사정거리
    public float attackDist = 2f;

    private int count = 0;

    // 사망 여부
    private bool isDead = false;
    private bool isHit = false;
    private bool isBlock = false;

    int nRand;

    float tempTime;

    bool noneArm = false;
    public bool CheckWeapon;
    public bool isCombo = false;

    private bool coroutineCheck = true;

    [Header("Cut Parts")]
    // 절단 부위 오브젝트
    public GameObject cut_LArm;
    public GameObject cut_RArm;
    public GameObject head;
    public GameObject lowerBody;

    [Header("Spawn Parts")]
    // 절단 부위 생성
    public GameObject spawnLArm;
    public GameObject spawnRArm;
    public GameObject spawnHead;
    public GameObject spawnLowerBody;

    [Header("Scripts")]
    public PlayerStat playerStat;
    public EasySurvivalScripts.PlayerMovement playerManager;
    public EnemyStat enemyStat;

    void Start()
    {
        // Enemy란 태그에 체력, 공격력 설정해줌
        //if (gameObject.CompareTag("Enemy"))
        //{
        //    SetEnemyStatus(nowHP, attackDmg);
        //}

        transform = this.gameObject.GetComponent<Transform>();
        playerTransform = GameObject.FindWithTag("Player").GetComponent<Transform>(); // Player란 태그를 찾음
        nvAgent = this.gameObject.GetComponent<NavMeshAgent>();
        animator = this.gameObject.GetComponent<Animator>();

        tempTime = Time.time;

        // 추적 대상의 위치를 설정하면 바로 추적 시작
        //nvAgent.destination = playertransform.position;

        StartCoroutine(this.CheckState());
        StartCoroutine(this.CheckStateForAction());

        count = 0;
    }

    void Update()
    {
        nRand = Random.Range(0, 100);

        //if (playerTransform != null) // 플레이어 쳐다보기
        //{
        //    Vector3 target = playerTransform.position - transform.position;
        //    Vector3 lookTarget = Vector3.Slerp(transform.forward, target.normalized, Time.deltaTime * 3);
        //    transform.rotation = Quaternion.LookRotation(lookTarget);
        //}
    }

    IEnumerator CheckState()
    {
        while (isDead == false)
        {
            yield return new WaitForSeconds(0.2f);

            float dist = Vector3.Distance(playerTransform.position, transform.position);

            if (dist <= attackDist)
            {
                curState = CurrentState.attack;
            }
            else if (dist <= traceDist)
            {
                curState = CurrentState.trace;
            }
            else
            {
                curState = CurrentState.idle;
            }
        }
    }

    IEnumerator CheckStateForAction()
    {
        while (isDead == false)
        {
            switch(curState)
            {
                case CurrentState.idle:
                    nvAgent.isStopped = true;
                    animator.SetBool("isTrace", false);
                    break;

                case CurrentState.trace:
                    if (playerTransform.CompareTag("Player") && isHit == false)
                    {
                        nvAgent.destination = playerTransform.position;
                        nvAgent.isStopped = false;
                        nvAgent.updatePosition = true;
                        nvAgent.updateRotation = true;
                        animator.SetBool("isTrace", true);
                        if (playerTransform != null) // 플레이어 쳐다보기
                        {
                            Vector3 target = playerTransform.position - transform.position;
                            Vector3 lookTarget = Vector3.Slerp(transform.forward, target.normalized, Time.deltaTime * 3);
                            transform.rotation = Quaternion.LookRotation(lookTarget);
                        }
                    }
                    else if (playerTransform.CompareTag("Untagged")) // 부쉬에 숨으면 플레이어 태그변경으로 추적불가능
                    {
                        nvAgent.isStopped = true;
                        animator.SetBool("isTrace", false);
                    }
                    break;

                case CurrentState.attack:
                    animator.SetBool("isTrace", false);
                    nvAgent.isStopped = true;
                    if (playerTransform != null) // 플레이어 쳐다보기
                    {
                        Vector3 target = playerTransform.position - transform.position;
                        Vector3 lookTarget = Vector3.Slerp(transform.forward, target.normalized, Time.deltaTime * 3);
                        transform.rotation = Quaternion.LookRotation(lookTarget);
                    }             
                    if (tempTime <= Time.time && playerStat.nowHP > 0 && noneArm == false && CheckWeapon == false)
                    {
                        tempTime = Time.time + 2;
                        animator.SetTrigger("Attack");
                        count++;
                    }
                    else if (tempTime <= Time.time && playerStat.nowHP > 0 && noneArm == true)
                    {
                        tempTime = Time.time + 2;
                        animator.SetTrigger("Kick");
                    }
                    else if (tempTime <= Time.time && playerStat.nowHP > 0 && noneArm == false && CheckWeapon == true && isCombo == false)
                    {
                        tempTime = Time.time + 4;
                        animator.SetTrigger("WeaponAttack");
                    }
                    if (tempTime <= Time.time && playerStat.nowHP > 0 && noneArm == false && isCombo == true && count == 0)
                    {
                        tempTime = Time.time + 2;
                        animator.SetTrigger("Combo");
                        count++;
                    }
                    else if (tempTime <= Time.time && playerStat.nowHP > 0 && noneArm == false && isCombo == true && count == 1)
                    {
                        tempTime = Time.time + 2;
                        animator.SetTrigger("Combo2");
                        count++;
                    }
                    else if (tempTime <= Time.time && playerStat.nowHP > 0 && noneArm == false && isCombo == true && count == 2)
                    {
                        tempTime = Time.time + 2;
                        animator.SetTrigger("Combo3");
                        count++;
                    }
                    else if (tempTime <= Time.time && playerStat.nowHP > 0 && noneArm == false && isCombo == true && count == 3)
                    {
                        tempTime = Time.time + 2;
                        animator.SetTrigger("Combo4");
                        count = 0;
                    }
                    break;

                case CurrentState.hit:
                    animator.SetTrigger("hit");
                    if (isHit == true)
                    {
                        nvAgent.isStopped = true;
                        nvAgent.updatePosition = false;
                        nvAgent.updateRotation = false;
                        nvAgent.velocity = Vector3.zero;
                    }
                    break;

                case CurrentState.block:
                    break;

                case CurrentState.dead:
                    animator.SetTrigger("death");
                    // Ragdoll Code
                    //ChangeRagdoll();
                    //Instantiate(ragdollObj, transform.position, transform.rotation);
                    //Spine.AddForce(new Vector3(0f, 0f, 1000f));
                    //
                    isDead = true;
                    Destroy(gameObject, 5f);
                    this.enabled = false; // 죽어도 닿으면 데미지받아서 스크립트 끔
                    break;

                case CurrentState.finish:
                    playerManager.anim.SetTrigger("Finish");
                    animator.SetTrigger("Finish");
                    isDead = true;
                    Destroy(gameObject, 5f);
                    this.enabled = false;
                    break;

                case CurrentState.kick:
                    animator.SetTrigger("Kicked");
                    if (isHit == true)
                    {
                        nvAgent.isStopped = true;
                        nvAgent.updatePosition = false;
                        nvAgent.updateRotation = false;
                        nvAgent.velocity = Vector3.zero;
                    }
                    break;
            }

            yield return null;
        }
    }

    // 플레이어의 공격을 받는지 충돌체크
    private void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.CompareTag("Cut") && isDead == false)
        {
            if (coroutineCheck == true)
            {
                coroutineCheck = false;
                StartCoroutine(AttackDelay(coll));
            }

            // 잘리는지 체크
            if (playerManager.HeadCut == true && head.tag == "CutOn" && nRand < 25)
            {
                CutHead();
                playerManager.HeadCut = false;
            }
            else if (playerManager.LeftCut == true && cut_LArm.tag == "CutOn" && nRand < 35)
            {
                CutLeftArm();
                playerManager.LeftCut = false;
            }
            else if (playerManager.RightCut == true && cut_RArm.tag == "CutOn" && nRand < 25)
            {
                CutRightArm();
                playerManager.RightCut = false;
            }
            else if (playerManager.LowerBody == true && lowerBody.tag == "CutOn" && nRand < 15)
            {
                CutLowerBody();
                playerManager.LowerBody = false;
            }
        }
        if (coll.gameObject.layer == LayerMask.NameToLayer("Player")) // 발차기 충돌체크 할려고 만듬
        {
            if (coroutineCheck == true)
            {
                coroutineCheck = false;
                StartCoroutine(AttackDelay(coll));
            }
        }
    }

    IEnumerator AttackDelay(Collision collision) // 공격 받는 판정 딜레이해주는 코루틴
    {
        if (enemyStat.nowHP > 0 && isBlock == false)
        {
            curState = CurrentState.hit;
            HitEffect();
            enemyStat.nowHP -= playerStat.attackDmg;
            Debug.Log("적 HP : " + enemyStat.nowHP);
        }

        if (enemyStat.nowHP <= 0)
        {
            curState = CurrentState.dead; // 죽으면 상태 전환
        }

        if (enemyStat.nowHP > 0 && playerManager.isFinish == true) // 피니쉬 모션
        {
            curState = CurrentState.finish;
            HitEffect();
            playerManager.isFinish = false;
        }

        if (enemyStat.nowHP > 0 && playerManager.isKick == true)  // 발차기 맞은 모션
        {
            curState = CurrentState.kick;
            playerManager.isKick = false;

            print("Kick");
        }

        yield return new WaitForSeconds(0.1f);

        coroutineCheck = true;
    }

    void HitEffect()
    {
        hitParticle.Play();
        hitParticle2.Play();
        hitParticle3.Play();

        Vector3 dir = playerBlade.position - transform.position;
        Vector3 dir2 = playerBlade.position - transform.position;
        Vector3 dir3 = playerBlade.position - transform.position;

        hitParticle.transform.position = transform.position + dir.normalized;
        hitParticle.transform.rotation = Quaternion.LookRotation(dir);
        hitParticle2.transform.position = transform.position + dir2.normalized;
        hitParticle2.transform.rotation = Quaternion.LookRotation(dir2);
        hitParticle3.transform.position = transform.position + dir3.normalized;
        hitParticle3.transform.rotation = Quaternion.LookRotation(dir3);

        Instantiate(hitParticle, hitParticle.transform.position, hitParticle.transform.rotation);
        Instantiate(hitParticle2, hitParticle2.transform.position, hitParticle2.transform.rotation);
        Instantiate(hitParticle3, hitParticle3.transform.position, hitParticle3.transform.rotation);
    }

    #region Animation Event Func
    // 애니메이션 클립에 사용할 함수
    void OnAttackColl() // 공격할때 콜라이더 켜기
    {
        coll.enabled = true;
    }

    void OffAttackColl() // 끄기
    {
        coll.enabled = false;
    }

    void OnKickColl()
    {
        legColl.enabled = true;
    }

    void OffKickColl()
    {
        legColl.enabled = false;
    }

    void OnWeaponColl()
    {
        WeaponColl.enabled = true;
    }

    void OffWeaponColl()
    {
        WeaponColl.enabled = false;
    }

    void OnHit()
    {
        isHit = true;
    }

    void OffHit()
    {
        isHit = false;
    }

    void OnSlowMotion()
    {
        if (Time.timeScale == 1.0f)
        {
            Time.timeScale = 0.5f;
        }
    }

    void OffSlowMotion()
    {
        if (Time.timeScale != 1.0f)
        {
            Time.timeScale = 1.0f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
    }

    #endregion

    // 절단 부위 꺼주기
    void CutRightArm()
    {
        cut_RArm.SetActive(false);
        Instantiate(spawnRArm, transform.position, transform.rotation);
        cut_RArm.tag = "CutOff";           // 태그를 변경해서 한번만 작동하게끔
        coll.gameObject.SetActive(false);  // 오른팔 잘리면 공격못하게 공격 판정 콜라이더 끄기

        noneArm = true;                    // 공격할 팔이 없으면 true
        attackDist = 1.5f;                 // 발 공격 거리에 맞게 attackDist 값 변경
    }

    void CutLeftArm()
    {
        cut_LArm.SetActive(false);
        Instantiate(spawnLArm, transform.position, transform.rotation);
        cut_LArm.tag = "CutOff";
    }

    void CutHead()
    {
        head.SetActive(false);
        Instantiate(spawnHead, transform.position, transform.rotation);
        head.tag = "CutOff";

        traceDist = 0f; // 머리 잘리면 안보이니까 추적 못하게
    }

    void CutLowerBody()
    {
        lowerBody.SetActive(false);
        Instantiate(spawnLowerBody, transform.position, transform.rotation);
        lowerBody.tag = "CutOff";

        animator.speed = 2f;
        curState = CurrentState.dead; // 즉사
    }
}