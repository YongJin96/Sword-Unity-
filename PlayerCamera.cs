using System.Collections;
using UnityEngine;

namespace EasySurvivalScripts
{
    public enum CameraPerspective
    {
        FirstPerson,
        ThirdPerson
    }

    public class PlayerCamera : MonoBehaviour
    {

        [Header("Input Settings")]
        public string MouseXInput;
        public string MouseYInput;
        public string SwitchPerspectiveInput;

        [Header("Common Camera Settings")]
        public float mouseSensitivity;
        public CameraPerspective cameraPerspective;

        [Header("Character Animator")]
        public Animator CharacterAnimator;

        [Header("FPS Camera Settings")]
        public Vector3 FPS_CameraOffset;
        public Vector2 FPS_MinMaxAngles;

        [Header("TPS Camera Settings")]
        public Vector3 TPS_CameraOffset;
        public Vector2 TPS_MinMaxAngles;

        Transform FPSController;
        float xClamp;
        Vector3 camMoveLoc;
        Transform _fpsCameraHelper;
        Transform _tpsCameraHelper;

        // Camera Shake
        [Header("Camera Shake Setting")]
        public AnimationCurve ShakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        public float Duration = 2;
        public float Speed = 22;
        public float Magnitude = 1;
        public float DistanceForce = 100;
        public float RotationDamper = 2;
        public bool IsEnabled = true;

        public PlayerStat playerStat;
        public Transform FromPos;
        public Transform ToPos;

        bool isPlaying;
        [HideInInspector]
        public bool canUpdate;
        //

        // 카메라 충돌 체크
        [Header("Wall Obstacle Setting")]
        public Transform target;               // 가야할 위치
        public Transform tr;                   // 돌아올 위치

        public float colliderRadius = 1f;      // 충돌처리 반지름
        public float zoomIn = 0;               // 충돌시 카메라 확대
        public float zoomOut = 0;              // 충돌시 카메라 축소
        public float zoomSpeed = 2;            // 카메라 확대/축소 속도

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            xClamp = 0;
            FPSController = GetComponentInParent<PlayerMovement>().transform;
        }

        // Use this for initialization
        void Start()
        {
            if (CharacterAnimator)
            {
                Add_FPSCamPositionHelper();
                Add_TPSCamPositionHelper();
            }
        }

        // My Code
        public void PlayShake()
        {
            StopAllCoroutines();
            StartCoroutine(Shake());
        }

        void OnEnable()
        {
            //isPlaying = true;
            var shakes = FindObjectsOfType(typeof(RFX4_CameraShake)) as RFX4_CameraShake[];
            if (shakes != null)
                foreach (var shake in shakes)
                {
                    shake.canUpdate = false;
                }
            canUpdate = true;
        }

        void Add_FPSCamPositionHelper()
        {
            _fpsCameraHelper = new GameObject().transform;
            _fpsCameraHelper.name = "_fpsCameraHelper";
            _fpsCameraHelper.SetParent(CharacterAnimator.GetBoneTransform(HumanBodyBones.Head));
            _fpsCameraHelper.localPosition = Vector3.zero;
        }


        void Add_TPSCamPositionHelper()
        {
            _tpsCameraHelper = new GameObject().transform;
            _tpsCameraHelper.name = "_tpsCameraHelper";
            _tpsCameraHelper.SetParent(FPSController);
            _tpsCameraHelper.localPosition = Vector3.zero;
        }

        // Update is called once per frame
        void Update()
        {
            //SwitchCameraPerspectiveInput();

            GetSetPerspective();

            RotateCamera();

            // 카메라가 흔들리면 다시 원위치로
            transform.rotation = Quaternion.Slerp(FromPos.rotation, ToPos.rotation, 0.1f);

            CameraColliderCheck();
        }


        private void CameraColliderCheck()
        {
            // 구체 형태의 충돌체로 충돌 여부를 검사
            if (Physics.CheckSphere(tr.position, colliderRadius, 1 << LayerMask.NameToLayer("Map")))
            {
                // 보간함수를 사용하여 카메라의 높이를 부드럽게 상승시킴
                //transform.position = Vector3.Lerp(transform.position, target.position, 0.5f);
                transform.position = Vector3.Lerp(transform.position, target.position, zoomIn * zoomSpeed);
                zoomIn += Time.deltaTime;
                zoomOut = 0;
            }
            else
            {
                // 보간함수를 사용하여 카메라의 높이를 부드럽게 하강시킴
                //transform.position = Vector3.Lerp(transform.position, tr.position, 0.5f);
                transform.position = Vector3.Lerp(target.position, tr.position, zoomOut * zoomSpeed);
                zoomOut += Time.deltaTime;
                zoomIn = 0;
            }

            // 주인공이 장애물에 가려졌는지를 판단할 레이캐스트의 높낮이를 설정
            //Vector3 castTarget = target.position + (target.up * castOffset);
            // castTarget 좌표로의 방향벡터를 계산
            //Vector3 castDir = (castTarget - tr.position).normalized;
            // 충돌 정보를 반할받을 변수
            //RaycastHit hit;

            // 레이캐스트를 투사해 장애물 여부를 검사
            //if (Physics.Raycast(tr.position, castDir, out hit, Mathf.Infinity))
            //{
            //    // 주인공이 레이캐스트에 맞지 않았을 경우
            //    if (!hit.collider.CompareTag("Player"))
            //    {
            //        // 보간함수를 사용하여 카메라의 높이를 부드럽게 상승시킴
            //        height = Mathf.Lerp(height, heightAboveObstacle, Time.deltaTime * overDamping);
            //        print("RayCast");
            //    }
            //    else
            //    {
            //        // 보간함수를 사용하여 카메라의 높이를 부드럽게 하상시킴
            //        height = Mathf.Lerp(height, originHeight, Time.deltaTime * overDamping);
            //    }
            //}
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(tr.position, colliderRadius);

            //Gizmos.color = Color.red;
            //Gizmos.DrawLine(target.position + (target.up * castOffset), tr.position);
        }

        void SwitchCameraPerspectiveInput()
        {
            if(Input.GetButtonDown(SwitchPerspectiveInput))
            {
                if (cameraPerspective == CameraPerspective.FirstPerson)
                {
                    cameraPerspective = CameraPerspective.ThirdPerson;
                }
                else
                {
                    cameraPerspective = CameraPerspective.FirstPerson;
                }
            }

        }

        void GetSetPerspective()
        {
            switch (cameraPerspective)
            {
                case CameraPerspective.FirstPerson:
                    SetCameraHelperPosition_FPS();
                    break;

                case CameraPerspective.ThirdPerson:
                    SetCameraHelperPosition_TPS();
                    break;
            }
        }

        void SetCameraHelperPosition_FPS()
        {
            if (!CharacterAnimator)
                return;

            _fpsCameraHelper.localPosition = FPS_CameraOffset;

            transform.position = _fpsCameraHelper.position;

        }

        void SetCameraHelperPosition_TPS()
        {
            if (!CharacterAnimator)
                return;

            _tpsCameraHelper.localPosition = TPS_CameraOffset;

            transform.position = _tpsCameraHelper.position;

        }

        void RotateCamera()
        {
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                mouseSensitivity += 50;
                print("마우스 민감도 = " + mouseSensitivity);
            }
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                mouseSensitivity -= 50;
                print("마우스 민감도 = " + mouseSensitivity);
            }

            float mouseX = Input.GetAxis(MouseXInput) * (mouseSensitivity * Time.deltaTime);
            float mouseY = Input.GetAxis(MouseYInput) * (mouseSensitivity * Time.deltaTime);
            Vector3 eulerRotation = transform.eulerAngles;
            xClamp += mouseY;

            if(cameraPerspective == CameraPerspective.FirstPerson)
                xClamp = Mathf.Clamp(xClamp, FPS_MinMaxAngles.x, FPS_MinMaxAngles.y);
            else
                xClamp = Mathf.Clamp(xClamp, TPS_MinMaxAngles.x, TPS_MinMaxAngles.y);

            eulerRotation.x = -xClamp;
            transform.eulerAngles = eulerRotation;
            FPSController.Rotate(Vector3.up * mouseX);  // X 좌표 카메라 회전
        }

        private void OnDrawGizmosSelected()
        {
            if (_fpsCameraHelper)
                Gizmos.DrawWireSphere(_fpsCameraHelper.position, 0.1f);

            Gizmos.color = Color.green;

            if (_tpsCameraHelper)
                Gizmos.DrawWireSphere(_tpsCameraHelper.position, 0.1f);
        }

        IEnumerator Shake()
        {
            var elapsed = 0.0f;
            var camT = Camera.main.transform;
            var originalCamRotation = camT.rotation.eulerAngles;
            var direction = (transform.position - camT.position).normalized;
            var time = 0f;
            var randomStart = Random.Range(-1000.0f, 1000.0f);
            var distanceDamper = 1 - Mathf.Clamp01((camT.position - transform.position).magnitude / DistanceForce);
            Vector3 oldRotation = Vector3.zero;
            while (elapsed < Duration && canUpdate)
            {
                elapsed += Time.deltaTime;
                var percentComplete = elapsed / Duration;
                var damper = ShakeCurve.Evaluate(percentComplete) * distanceDamper;
                time += Time.deltaTime * damper;
                camT.position -= direction * Time.deltaTime * Mathf.Sin(time * Speed) * damper * Magnitude / 2;

                var alpha = randomStart + Speed * percentComplete / 10;
                var x = Mathf.PerlinNoise(alpha, 0.0f) * 2.0f - 1.0f;
                var y = Mathf.PerlinNoise(1000 + alpha, alpha + 1000) * 2.0f - 1.0f;
                var z = Mathf.PerlinNoise(0.0f, alpha) * 2.0f - 1.0f;

                if (Quaternion.Euler(originalCamRotation + oldRotation) != camT.rotation)
                    originalCamRotation = camT.rotation.eulerAngles;
                oldRotation = Mathf.Sin(time * Speed) * damper * Magnitude * new Vector3(0.5f + y, 0.3f + x, 0.3f + z) * RotationDamper;
                camT.rotation = Quaternion.Euler(originalCamRotation + oldRotation);
     
                yield return null;
            }
        }
    }
}