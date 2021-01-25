using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMap : MonoBehaviour
{
    float MoveX;
    float MoveY;
    float MoveZ;

    float SaveX;
    float SaveY;
    float SaveZ;

    public float MoveSpeed = 3f;
    public float MoveLength = 3f;

    float RunTime = 0;

    public bool LeftRightMove = false;
    public bool LeftRightMove2 = false;
    public bool UpDownMove = false;

    public GameObject player;

    void Start()
    {
        SaveX = transform.position.x;
        SaveY = transform.position.y;
        SaveZ = transform.position.z;

        MoveX = transform.position.x;
        MoveY = transform.position.y;
        MoveZ = transform.position.z;
    }

    void Update()
    {
        if (LeftRightMove == true)
        {
            RunTime += Time.deltaTime * MoveSpeed;
            MoveX = Mathf.Sin(RunTime) * MoveLength;
            this.transform.position = new Vector3((SaveX + MoveX), MoveY, MoveZ);
        }
        if (LeftRightMove2 == true)
        {
            RunTime += Time.deltaTime * MoveSpeed;
            MoveZ = Mathf.Sin(RunTime) * MoveLength;
            this.transform.position = new Vector3(MoveX, MoveY, (SaveZ + MoveZ));
        }
        if (UpDownMove == true)
        {
            RunTime += Time.deltaTime * MoveSpeed;
            MoveY = Mathf.Sin(RunTime) * MoveLength;
            this.transform.position = new Vector3(MoveX, (SaveY + MoveY), MoveZ);
        }
    }

    //private void OnTriggerEnter(Collider coll)
    //{
    //    if (coll.gameObject.CompareTag("Player"))
    //    {
    //        player.transform.SetParent(this.transform);
    //    }   
    //}
    //
    //private void OnTriggerExit(Collider coll)
    //{
    //    if (coll.gameObject.CompareTag("Player"))
    //    {
    //        player.transform.SetParent(null);
    //    }
    //}

    private void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.CompareTag("Player"))
        {
            player.transform.SetParent(this.transform);
        }   
    }
    private void OnCollisionExit(Collision coll)
    {
        if (coll.gameObject.CompareTag("Player"))
        {
            player.transform.SetParent(null);
        }
    }
}
