using UnityEngine;

public class ClearObject : MonoBehaviour
{
    public float clearTime;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Destroy(gameObject, clearTime);
        }
    }
}
