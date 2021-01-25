using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateEffect : MonoBehaviour
{
    public ParticleSystem particle;

    private void OnCollisionEnter(Collision collision)
    {
        particle.Play();
        Vector3 dir = transform.position - transform.position;
        particle.transform.position = transform.position + dir.normalized;
        particle.transform.rotation = Quaternion.LookRotation(dir);

        Instantiate(particle, particle.transform.position, particle.transform.rotation);
    }
}
