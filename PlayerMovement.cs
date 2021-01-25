using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasySurvivalScripts
{
    public enum PlayerStates
    {
        Idle,
        Walking,
        Running,
        Jumping,
    }

    public class PlayerMovement : MonoBehaviour
    {
        public PlayerStates playerStates;

        public Animator anim;

        [Header("Inputs")]
        public string HorizontalInput = "Horizontal";
        public string VerticalInput = "Vertical";
        public string RunInput = "Run";
        public string JumpInput = "Jump";

        //public string AttackInput = "Attack";

        [Header("Player Motor")]
        [Range(1f,15f)]
        public float walkSpeed;
        [Range(1f,15f)]
        public float runSpeed;
        [Range(1f,15f)]
        public float JumpForce;

        // 경사면
        public float slopeForce;
        public float slopeForceRayLength;

        [Header("Animator and Parameters")]
        public Animator CharacterAnimator;
        public float HorzAnimation;
        public float VertAnimation;
        public bool JumpAnimation;
        public bool LandAnimation;

        CharacterController characterController;

        // My Code
        [Header("Objects")]
        public PlayerCamera playerCam;
        public MeshCollider coll;
        public BoxCollider KickColl;
        public GameObject OnBlade;  // RBlade
        public GameObject OffBlade; // LBlade
        [Header("Trails")]
        public GameObject SlashEffect;
        public GameObject SlashEffect2;
        public GameObject SlashEffect3;
        public GameObject SlashEffect4;
        public GameObject SlashEffect5;
        public GameObject SlashEffect6;
        public GameObject SlashEffect7;
        public GameObject SlashEffect8;
        public GameObject SlashEffect9;

        public GameObject SkillEffect;
        public GameObject SkillEffect2;

        public GameObject DashAttackEffect;
        public GameObject JumpAttackEffect;

        public GameObject Buff;

        float delayTime = 1f;
        float tmpTime;
        float tmpTime2;
        float skillTime;
        float buffTime;

        int tabCount;
        int attackCount;
        int effectCount = 0;

        bool isAttack = false; // 무기 꺼내야 공격할수 있게

        [HideInInspector]
        public bool isEvasion = false; // 회피 체크

        [HideInInspector]
        public bool isCrouch = false; // 웅크리고 있는지

        [HideInInspector]
        public bool isFinish = false;
        [HideInInspector]
        public bool isKick = false;

        [HideInInspector]
        public bool isShield = false;
        bool typeChange = false;

        // 자를수 있는 부위 체크해줄려고
        [HideInInspector]
        public bool HeadCut = false;
        [HideInInspector]
        public bool LeftCut = false;
        [HideInInspector]
        public bool RightCut = false;
        [HideInInspector]
        public bool LowerBody = false;
        // My Code

        // Feet IK Code
        private Vector3 rightFootPosition, leftFootPosition, leftFootIKPosition, rightFootIKPosition;
        private Quaternion leftFootIKRotation, rightFootIKRotation;
        private float lastPelvisPositionY, lastRightFootPostionY, lastLeftFootPostionY;

        [Header("FeetIK Settings")]
        public bool enableFeetIK = true;
        [Range(0, 2)] [SerializeField] private float heightFromGroundRaycast = 1.14f;
        [Range(0, 2)] [SerializeField] private float raycastDownDistance = 1.5f;
        [SerializeField] private LayerMask environmentLayer;
        [SerializeField] private float pelvisOffset = 0f;
        [Range(0, 2)] [SerializeField] private float pelvisUpAndDownSpeed = 0.28f;
        [Range(0, 2)] [SerializeField] private float feetToIkPostionSpeed = 0.5f;

        public string leftFootAnimVariableName = "LeftFootCurve";
        public string rightFootAnimVariableName = "RightFootCurve";

        public bool useProIKFeature = false;
        public bool showSolverDebug = true;
        // End Feet IK Code

        // Use this for initialization
        void Start()
        {
            characterController = GetComponent<CharacterController>();

            attackCount = 0;
            tabCount = 0;

            tmpTime = Time.time;
            tmpTime2 = Time.time;
            skillTime = Time.time;
            buffTime = Time.time;
        }

        // Update is called once per frame
        void Update()
        {
            //handle controller
            HandlePlayerControls();

            //sync animations with controller
            SetCharacterAnimations();
        }

        void HandlePlayerControls()
        {
            float hInput = Input.GetAxisRaw(HorizontalInput);
            float vInput = Input.GetAxisRaw(VerticalInput);

            Vector3 fwdMovement = characterController.isGrounded == true ? transform.forward * vInput : Vector3.zero;
            Vector3 rightMovement = characterController.isGrounded == true ? transform.right * hInput : Vector3.zero;

            float _speed = Input.GetButton(RunInput) ? runSpeed : walkSpeed;
            characterController.SimpleMove(Vector3.ClampMagnitude(fwdMovement + rightMovement, 1f) * _speed);

            // 경사면 내려갈때 중력을 쎄게줘서 튀는현상을 잡아줌
            if ((vInput != 0 || hInput != 0) && OnSlope())
            {
                characterController.Move(Vector3.down * characterController.height / 2 * slopeForce * Time.deltaTime);
            }

            if (characterController.isGrounded)
            {
                Jump();
                EquipWeapon();
                UnEquipWeapon();
                AttackSet();
                AttackSet2();
                StrongAttack();
                Skill();
                Slide();
                Crouch();
                Dodge();
                //Dash();
                FinishAttack();
                DashAttack();
                AttackType();
                WeaponCheck();
                Shield();
                Throw();
            }
            JumpAttack();
            
            //Managing Player States
            if (characterController.isGrounded)
            {
                if (hInput == 0 && vInput == 0)
                {
                    playerStates = PlayerStates.Idle;
                    enableFeetIK = true;
                }
                else
                {
                    if (_speed == walkSpeed)
                    {
                        playerStates = PlayerStates.Walking;
                        enableFeetIK = true;
                    }
                    else
                    {
                        playerStates = PlayerStates.Running;
                        enableFeetIK = true;
                    }
                }
            }
            else
            {
                playerStates = PlayerStates.Jumping;
            }
        }

        void Jump()
        {
            if (Input.GetButtonDown(JumpInput))
            {
                enableFeetIK = false;
                StartCoroutine(PerformJumpRoutine());
                JumpAnimation = true;
            }
        }

        #region Animation Event Func
        // 애니메이션 클립에서 사용할 함수
        // 공격 애니메이션때 판정 만들어주기
        void OnCollEnable()
        {
            coll.enabled = true;
        }

        // 애니메이션 도중 판정 다시 없애주기
        void OffCollEnable()
        {
            coll.enabled = false;
        }

        void OnEffect()
        {
            if (typeChange == false && effectCount == 0)
            {
                SlashEffect.gameObject.SetActive(true);
                effectCount++;
            }
            else if (typeChange == false && effectCount == 1)
            {
                SlashEffect2.gameObject.SetActive(true);
                effectCount++;
            }
            else if (typeChange == false && effectCount == 2)
            {
                SlashEffect3.gameObject.SetActive(true);
                effectCount++;
            }
            else if (typeChange == false && effectCount == 3)
            {
                SlashEffect4.gameObject.SetActive(true);
                effectCount = 0;
            }

            if (typeChange == true && effectCount == 0)
            {
                SlashEffect5.gameObject.SetActive(true);
                effectCount++;
            }
            else if (typeChange == true && effectCount == 1)
            {
                SlashEffect6.gameObject.SetActive(true);
                effectCount++;
            }
            else if (typeChange == true && effectCount == 2)
            {
                SlashEffect7.gameObject.SetActive(true);
                effectCount++;
            }
            else if (typeChange == true && effectCount == 3)
            {
                SlashEffect8.gameObject.SetActive(true);
                effectCount++;
            }
            else if (typeChange == true && effectCount == 4)
            {
                SlashEffect9.gameObject.SetActive(true);
                effectCount = 0;
            }
        }

        void OffEffect()
        {
            if (typeChange == false)
            {
                SlashEffect.gameObject.SetActive(false);
                SlashEffect2.gameObject.SetActive(false);
                SlashEffect3.gameObject.SetActive(false);
                SlashEffect4.gameObject.SetActive(false);
            }
            else if (typeChange == true)
            {
                SlashEffect5.gameObject.SetActive(false);
                SlashEffect6.gameObject.SetActive(false);
                SlashEffect7.gameObject.SetActive(false);
                SlashEffect8.gameObject.SetActive(false);
                SlashEffect9.gameObject.SetActive(false);
            }
        }

        void OnSkillEffect()
        {
            if (typeChange == false)
            {
                SkillEffect.gameObject.SetActive(true);
            }
            else if (typeChange == true)
            {
                SkillEffect2.gameObject.SetActive(true);
            }
        }

        void OffSkillEffect()
        {
            if (typeChange == false)
            {
                SkillEffect.gameObject.SetActive(false);
            }
            else if (typeChange == true)
            {
                SkillEffect2.gameObject.SetActive(false);
            }
        }

        void OnDashAttackEffect()
        {
            DashAttackEffect.gameObject.SetActive(true);
        }

        void OffDashAttackEffect()
        {
            DashAttackEffect.gameObject.SetActive(false);
        }

        void OnJumpAttackEffect()
        {
            JumpAttackEffect.gameObject.SetActive(true);
        }

        void OffJumpAttackEffect()
        {
            JumpAttackEffect.gameObject.SetActive(false);
        }

        void OnWeapon()
        {
            OnBlade.gameObject.SetActive(true);
            OffBlade.gameObject.SetActive(false);

        }

        void OffWeapon()
        {
            
            OnBlade.gameObject.SetActive(false);
            OffBlade.gameObject.SetActive(true);
        }

        void OnBuff()
        {
            Buff.SetActive(true);
        }

        void OffBuff()
        {
            Buff.SetActive(false);
        }

        void OnKick()
        {
            KickColl.enabled = true;
        }

        void OffKick()
        {
            KickColl.enabled = false;
        }

        void OnEvasion() // 회피
        {
            isEvasion = true;
        }

        void OffEvasion()
        {
            isEvasion = false;
        }

        #endregion

        public void AttackSet() // 공격타입 1
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && !Input.GetKey(KeyCode.LeftShift) && isAttack == true && typeChange == false)
            {
                // n(0.7)초 만큼 딜레이 작동 (중복실행안되게)
                if (tmpTime <= Time.time && attackCount == 0)
                {
                    tmpTime = Time.time + 0.5f;
                    anim.SetTrigger("Attack");
                    ++attackCount;

                    HeadCut = false;
                    LowerBody = false;
                    LeftCut = false;
                    RightCut = false;

                    isKick = false;
                    isFinish = false;
                }
                else if (tmpTime <= Time.time && attackCount == 1)
                {
                    tmpTime = Time.time + 0.5f;
                    anim.SetTrigger("Attack2");
                    ++attackCount;

                    HeadCut = false;
                    LowerBody = false;
                    LeftCut = true;      // true
                    RightCut = false;

                    isKick = false;
                    isFinish = false;
                }
                else if (tmpTime <= Time.time && attackCount == 2)
                {
                    tmpTime = Time.time + 0.5f;
                    anim.SetTrigger("Attack3");
                    ++attackCount;

                    HeadCut = false;
                    LowerBody = false;
                    LeftCut = false;
                    RightCut = true;    // true

                    isKick = false;
                    isFinish = false;
                }
                else if (tmpTime <= Time.time && attackCount == 3)
                {
                    tmpTime = Time.time + 0.7f;
                    anim.SetTrigger("Attack4");
                    attackCount = 0;

                    HeadCut = false;
                    LowerBody = false;
                    LeftCut = false;
                    RightCut = false;

                    isKick = false;
                    isFinish = false;
                }
            }
        }

        void AttackSet2() // 공격타입 2
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && !Input.GetKey(KeyCode.LeftShift) && typeChange == true && isAttack == true)
            {
                if (tmpTime <= Time.time && attackCount == 0)
                {
                    tmpTime = Time.time + 0.5f;
                    anim.SetTrigger("Attack5");
                    ++attackCount;

                    HeadCut = false;
                    LowerBody = false;
                    LeftCut = true;     // true
                    RightCut = false;

                    isKick = false;
                    isFinish = false;
                }
                else if (tmpTime <= Time.time && attackCount == 1)
                {
                    tmpTime = Time.time + 0.5f;
                    anim.SetTrigger("Attack6");
                    ++attackCount;

                    HeadCut = false;
                    LowerBody = false;
                    LeftCut = false;
                    RightCut = true;   // true

                    isKick = false;
                    isFinish = false;
                }
                else if (tmpTime <= Time.time && attackCount == 2)
                {
                    tmpTime = Time.time + 0.5f;
                    anim.SetTrigger("Attack7");
                    ++attackCount;

                    HeadCut = false;
                    LowerBody = false;
                    LeftCut = false;
                    RightCut = false;

                    isKick = false;
                    isFinish = false;
                }
                else if (tmpTime <= Time.time && attackCount == 3)
                {
                    tmpTime = Time.time + 0.5f;
                    anim.SetTrigger("Attack8");
                    ++attackCount;

                    HeadCut = false;
                    LowerBody = false;
                    LeftCut = false;
                    RightCut = true;    // true

                    isKick = false;
                    isFinish = false;
                }
                else if (tmpTime <= Time.time && attackCount == 4)
                {
                    tmpTime = Time.time + 0.5f;
                    anim.SetTrigger("Attack9");
                    attackCount = 0;

                    HeadCut = false;
                    LowerBody = false;
                    LeftCut = false;
                    RightCut = false;

                    isKick = true;      // true
                    isFinish = false;
                }        
            }
        }

        void AttackType()
        {
            if (Input.GetKeyDown(KeyCode.Tab) && isAttack == true && tabCount == 0) // Type 1
            {
                if (buffTime <= Time.time)
                {
                    buffTime = Time.time + 2;
                    anim.SetTrigger("Buff");
                    print("Type B");
                    typeChange = true;
                    ++tabCount;
                    attackCount = 0; // 공격 콤보 초기화
                    effectCount = 0;
                }
            }
            else if (Input.GetKeyDown(KeyCode.Tab) && isAttack == true && tabCount == 1) // Type 2
            {
                if (buffTime <= Time.time)
                {
                    buffTime = Time.time + 2;
                    anim.SetTrigger("Buff");
                    print("Type A");
                    typeChange = false;
                    tabCount = 0;
                    attackCount = 0; // 공격 콤보 초기화
                    effectCount = 0;
                }
            }
        }

        void StrongAttack()
        {
            if (Input.GetKeyDown(KeyCode.R) && isAttack == true && tabCount == 0) // Type 1
            {
                if (tmpTime2 <= Time.time)
                {
                    tmpTime2 = Time.time + 2;
                    anim.SetTrigger("StrongAttack1");
                    attackCount = 0;
                    HeadCut = true;

                    LowerBody = false;
                    LeftCut = false;
                    RightCut = false;

                    isKick = false;
                    isFinish = false;
                    effectCount = 0;
                }
            }
            else if (Input.GetKeyDown(KeyCode.R) && isAttack == true && tabCount == 1) // Type 2
            {
                if (tmpTime2 <= Time.time)
                {
                    tmpTime2 = Time.time + 2;
                    anim.SetTrigger("StrongAttack2");
                    attackCount = 0;
                    LowerBody = true;

                    HeadCut = false;
                    LeftCut = false;
                    RightCut = false;

                    isKick = false;
                    isFinish = false;
                    effectCount = 0;
                }
            }
        }

        void DashAttack()
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W) && Input.GetKeyDown(KeyCode.Mouse0) && isAttack == true)
            {
                if (tmpTime2 <= Time.time)
                {
                    tmpTime2 = Time.time + 1;
                    anim.SetTrigger("DashAttack");
                    attackCount = 0;
                    HeadCut = true;

                    LowerBody = false;
                    LeftCut = false;
                    RightCut = false;

                    isKick = false;
                    isFinish = false;
                    effectCount = 0;
                }
            }
        }

        void JumpAttack()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && isAttack == true && playerStates == PlayerStates.Jumping)
            {
                if (tmpTime2 <= Time.time)
                {
                    tmpTime2 = Time.time + 0.7f;
                    anim.SetTrigger("JumpAttack");
                    attackCount = 0;

                    //characterController.Move(Vector3.down * 60 * Time.deltaTime);

                    HeadCut = true;

                    LowerBody = false;
                    LeftCut = false;
                    RightCut = false;

                    isKick = false;
                    isFinish = false;
                    effectCount = 0;
                }
            }
        }

        public void FinishAttack()
        {
            if (Input.GetKeyDown(KeyCode.F) && isAttack == true)
            {
                if (tmpTime2 <= Time.time)
                {
                    tmpTime2 = Time.time + 1;
                    anim.SetTrigger("FinishAttack");
                    isFinish = true;
                }
            }
        }

        void FinishAttack2()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && isAttack == true && playerStates == PlayerStates.Jumping)
            {
                if (tmpTime2 <= Time.time)
                {
                    tmpTime2 = Time.time + 0.7f;
                    anim.SetTrigger("JumpAttack");
                    attackCount = 0;

                    HeadCut = true;

                    LowerBody = false;
                    LeftCut = false;
                    RightCut = false;

                    isFinish = false;
                }
            }
        }

        void Skill()
        {
            if (Input.GetKeyDown("t"))
            {
                if (tmpTime <= Time.time)
                {
                    tmpTime = Time.time + delayTime;
                    anim.SetTrigger("Skill");

                    HeadCut = false;

                    LowerBody = false;
                    LeftCut = false;
                    RightCut = false;

                    isKick = false;
                    isFinish = false;
                }
            }
        }

        public void Shield()
        {
            if (Input.GetKey(KeyCode.Mouse1) && isAttack == true)
            {
                anim.SetTrigger("Shield");
                isShield = true;
            }
            else if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                isShield = false;
            }
        }
        
        public void EquipWeapon()
        {
            
            if (Input.GetKeyDown("2"))
            {
                if (tmpTime <= Time.time)
                {
                    tmpTime = Time.time + delayTime;
                    anim.SetTrigger("Equip");
                }

                isAttack = true; // 무기를 꺼내면 공격 가능하게
            }
        }

       public void UnEquipWeapon()
        {
            if (Input.GetKeyDown("1"))
            {
                if (tmpTime <= Time.time)
                {
                    tmpTime = Time.time + delayTime;
                    anim.SetTrigger("Unequip");                
                    attackCount = 0;
                }

                isAttack = false; // 무기를 집어넣으면 공격 불가능하게
            }
        }

        void WeaponCheck()
        {
            if (Input.GetKeyDown("1"))
            {
                anim.SetBool("isWeapon", false);
            }
            if (Input.GetKeyDown("2"))
            {
                anim.SetBool("isWeapon", true);
            }
        }

        void Slide()
        {
            if (Input.GetKeyDown("c") && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W) && isCrouch == false)
            {
                if (tmpTime <= Time.time)
                {
                    tmpTime = Time.time + delayTime;
                    anim.SetTrigger("Slide");

                    attackCount = 0;
                }
            }
        }

        void Crouch()
        {
            if (Input.GetKeyDown(KeyCode.C) && !Input.GetKey(KeyCode.LeftShift) && isCrouch == false)
            {
                anim.SetBool("isCrouch", true);
                isCrouch = true;
            }
            else if (Input.GetKeyDown(KeyCode.C) && !Input.GetKey(KeyCode.LeftShift) && isCrouch == true)
            {
                anim.SetBool("isCrouch", false);
                isCrouch = false;
            }
        }

        void Dodge()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                if (tmpTime <= Time.time)
                {
                    tmpTime = Time.time + 0.7f;
                    anim.SetTrigger("Dodge");
                    
                }
            }  
        }

        void Dash()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                if (tmpTime <= Time.time)
                {
                    tmpTime = Time.time + 0.7f;
                    anim.SetTrigger("Dash");
                }
            }
        }

        void Throw()
        {
            if(Input.GetKeyDown("q"))
            {   
                if (tmpTime <= Time.time)
                {
                    tmpTime = Time.time + delayTime;
                    anim.SetTrigger("Throw");
                }
            }
        }

        public void PickFloor()
        {
            anim.SetTrigger("PickFloor");
        }

        public void Hit()
        {
            anim.SetTrigger("Hit");
            playerCam.PlayShake();
        }

        public void BlockHit()
        {
            anim.SetTrigger("BlockHit");
            playerCam.PlayShake();
        }

        public void Die()
        {
            anim.SetTrigger("Die");
            this.enabled = false;
            
        }

        #region FeetGrounding
        private void FixedUpdate()
        {
            if (enableFeetIK == false) { return; }
            if (anim == null) { return; }

            AdjustFeetTarget(ref rightFootPosition, HumanBodyBones.RightFoot);
            AdjustFeetTarget(ref leftFootPosition, HumanBodyBones.LeftFoot);

            FeetPostionSolver(rightFootPosition, ref rightFootIKPosition, ref rightFootIKRotation);
            FeetPostionSolver(leftFootPosition, ref leftFootIKPosition, ref leftFootIKRotation);

        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (enableFeetIK == false) { return; }
            if (anim == null) { return; }

            MovePelvisHeight();

            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);

            if (useProIKFeature)
            {
                anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, anim.GetFloat(rightFootAnimVariableName));
            }

            MoveFeetToIkPoint(AvatarIKGoal.RightFoot, rightFootIKPosition, rightFootIKRotation, ref lastRightFootPostionY);

            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);

            if (useProIKFeature)
            {
                anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, anim.GetFloat(leftFootAnimVariableName));
            }

            MoveFeetToIkPoint(AvatarIKGoal.LeftFoot, leftFootIKPosition, leftFootIKRotation, ref lastLeftFootPostionY);
        }

        #endregion

        #region FeetGroundingMethods

        void MoveFeetToIkPoint (AvatarIKGoal foot, Vector3 positionIKHolder, Quaternion rotationIKHolder, ref float lastFootPositionY)
        {
            Vector3 targetIKPosition = anim.GetIKPosition(foot);

            if (positionIKHolder != Vector3.zero)
            {
                targetIKPosition = transform.InverseTransformPoint(targetIKPosition);
                positionIKHolder = transform.InverseTransformPoint(positionIKHolder);

                float yVariable = Mathf.Lerp(lastFootPositionY, positionIKHolder.y, feetToIkPostionSpeed);
                targetIKPosition.y += yVariable;

                lastFootPositionY = yVariable;

                targetIKPosition = transform.TransformPoint(targetIKPosition);

                anim.SetIKRotation(foot, rotationIKHolder);
            }

            anim.SetIKPosition(foot, targetIKPosition);
        }

        private void MovePelvisHeight()
        {
            if (rightFootIKPosition == Vector3.zero || leftFootIKPosition == Vector3.zero || lastPelvisPositionY == 0)
            {
                lastPelvisPositionY = anim.bodyPosition.y;
                return;
            }

            float lOffsetPosition = leftFootIKPosition.y - transform.position.y;
            float rOffsetPosition = rightFootIKPosition.y - transform.position.y;

            float totalOffset = (lOffsetPosition < rOffsetPosition) ? lOffsetPosition : rOffsetPosition;

            Vector3 newPelvisPosition = anim.bodyPosition + Vector3.up * totalOffset;

            newPelvisPosition.y = Mathf.Lerp(lastPelvisPositionY, newPelvisPosition.y, pelvisUpAndDownSpeed);

            anim.bodyPosition = newPelvisPosition;

            lastPelvisPositionY = anim.bodyPosition.y;
        }

        void FeetPostionSolver (Vector3 fromSkyPosition, ref Vector3 feetIKPostions, ref Quaternion feetIKRotations)
        {
            RaycastHit feetOutHit;

            if (showSolverDebug)
            {
                Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down * (raycastDownDistance + heightFromGroundRaycast), Color.yellow);
            }

            if (Physics.Raycast(fromSkyPosition, Vector3.down, out feetOutHit, raycastDownDistance + heightFromGroundRaycast, environmentLayer))
            {
                feetIKPostions = fromSkyPosition;
                feetIKPostions.y = feetOutHit.point.y + pelvisOffset;
                feetIKRotations = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation;

                return;
            }

            feetIKPostions = Vector3.zero;
        }

        void AdjustFeetTarget (ref Vector3 feetPostions, HumanBodyBones foot)
        {
            feetPostions = anim.GetBoneTransform(foot).position;
            feetPostions.y = transform.position.y + heightFromGroundRaycast;
        }

        #endregion

        IEnumerator PerformJumpRoutine()
        {
            float _jump = JumpForce;

            do
            {
                characterController.Move(Vector3.up * _jump * Time.deltaTime);
                _jump -= Time.deltaTime * 2;
                yield return null;
            }
            while (!characterController.isGrounded);         
        }

        void SetCharacterAnimations()
        {
            if (!CharacterAnimator)
                return;

            switch (playerStates)
            {
                case PlayerStates.Idle:
                    HorzAnimation = Mathf.Lerp(HorzAnimation, 0, 5 * Time.deltaTime);
                    VertAnimation = Mathf.Lerp(VertAnimation, 0, 5 * Time.deltaTime);
                    break;

                case PlayerStates.Walking:
                    HorzAnimation = Mathf.Lerp(HorzAnimation, 1 * Input.GetAxis("Horizontal"), 5 * Time.deltaTime);
                    VertAnimation = Mathf.Lerp(VertAnimation, 1 * Input.GetAxis("Vertical"), 5 * Time.deltaTime);
                    break;

                case PlayerStates.Running:
                    HorzAnimation = Mathf.Lerp(HorzAnimation, 2 * Input.GetAxis("Horizontal"), 5 * Time.deltaTime);
                    VertAnimation = Mathf.Lerp(VertAnimation, 2 * Input.GetAxis("Vertical"), 5 * Time.deltaTime);
                    break;

                case PlayerStates.Jumping:
                    if (JumpAnimation)
                    {
                        CharacterAnimator.SetTrigger("Jump");
                        JumpAnimation = false;
                    }
                    break;
            }

            LandAnimation = characterController.isGrounded;
            CharacterAnimator.SetFloat("Horizontal", HorzAnimation);
            CharacterAnimator.SetFloat("Vertical", VertAnimation);
            CharacterAnimator.SetBool("isGrounded", LandAnimation);
        }
        
        private bool OnSlope()
        {
            if (playerStates == PlayerStates.Jumping)
                return false;

            RaycastHit hit;

            if (Physics.Raycast(transform.position, Vector3.down, out hit, characterController.height / 2 * slopeForceRayLength))
            {
                if (hit.normal != Vector3.up)
                    return true;
            }

            return false;
        }
    }
}