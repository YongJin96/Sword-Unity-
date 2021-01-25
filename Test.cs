using System.Collections;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Test : MonoBehaviour
{
    public Material color;         // 잘린부분 단면 색
    public GameObject weapon;      // 사용할 무기
    public Transform targetPos;    // 오브젝트를 넣으면 오브젝트와 닿는 위치를 알려줌
    //public Transform rightHandPos; // 오른손 위치 알기위해
    public GameObject character;   

    private Vector3 saveRot;
    private Vector3 savePos;

    private bool coroutineCheck = true;
    private bool IsAttack = false;

    void Start()
    {
        // 처음 위치를 저장해줌
        //saveRot = transform.rotation.eulerAngles;
        //savePos = targetPos.localPosition;
    }

    void Update()
    {
        EquipWeapon();
        UnEquipWeapon();
    }

    public void UnEquipWeapon()
    {
        if (Input.GetKeyDown("1"))
        {
            //weapon.transform.SetParent(character.transform);
        }
    }

    public void EquipWeapon()
    {
        if (Input.GetKeyDown("2"))
        {
            //weapon.transform.SetParent(rightHandPos.transform);            // rightHandPos를 weapon의 부모로
            //weapon.transform.localPosition = Vector3.zero;                 // localPosition은 부모의 위치값을 기준으로 설정
            //weapon.transform.localRotation = Quaternion.Euler(180, 0, 0);  // localRotation은 부모의 회전값을 기준으로 설정 // 180도 한 이유는 콜라이더가 180도 돌아가서 원위치할려고
        }
    }

    // Cut태그가 collision에 닿으면 CheckCol이란 코루틴함수를 실행함
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Cut"))
        {
            if (coroutineCheck == true)
            {
                coroutineCheck = false;
                StartCoroutine(CheckCol(collision));
            }
        }
    }

    IEnumerator CheckCol(Collision collision)
    {
        Vector3 first = transform.forward;
        Vector3 second = targetPos.position - collision.transform.position;

        GameObject[] temp = MeshCut.Cut(collision.gameObject, transform.position, Vector3.Cross(first, second), color);

        // 잘린 부분을 리지드 바디랑 메쉬 콜라이더추가해주고 태그 변경해주기
        for (int i = 0; i < temp.Length; i++)
        {
            temp[i].AddComponent<Rigidbody>();                  // rigidbody 컴포넌트 추가
            temp[i].AddComponent<MeshCollider>().convex = true; // 메쉬콜라이더에 convex를 체크해줌
            temp[i].gameObject.tag = "Cut";                     // 태그명 변경해주기
            temp[i].gameObject.layer = 11;

            Destroy(temp[i].gameObject, 15f);
        } 

        yield return new WaitForSeconds(0.2f);

        coroutineCheck = true;
    }
}
