using UnityEngine;

public class Throw : MonoBehaviour
{
    public GameObject throwObject;
    public float forceMultiplier;
    public Camera cam;
    public Transform throwPostion;

    float tempTime;

    void Start()
    {
        //cam = GetComponent<Camera>();

        tempTime = Time.time;
    }

    void Update()
    {
        Throwing();
    }

    void Throwing()
    {
        if (Input.GetKeyDown("q"))
        {
            if (tempTime <= Time.time)
            {
                tempTime = Time.time + 1f;
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                GameObject newThrowObject = Instantiate(throwObject, throwPostion.position, Quaternion.Euler(Vector3.zero));
                newThrowObject.GetComponent<Rigidbody>().AddForce(ray.direction * forceMultiplier);
            }
        }
    }
}
