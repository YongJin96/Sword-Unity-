using UnityEngine;

public class Trap : MonoBehaviour
{
    public Rigidbody trapObject;

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.CompareTag("Player"))
        {
            trapObject.isKinematic = false;
        }
    }
}
