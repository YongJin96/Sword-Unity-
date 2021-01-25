using UnityEngine;

public class Clear : MonoBehaviour
{
    public float ClearTime = 5f;

    private void Start()
    {
        Destroy(gameObject, ClearTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Map"))
        {
            Destroy(gameObject);
        } 

        if (gameObject.layer == LayerMask.NameToLayer("Throw"))
        {
            Destroy(gameObject);
        }
    }
}
