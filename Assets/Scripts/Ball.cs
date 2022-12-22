using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField] private float pushForce = 40f; //How much force is applied to ball if its pushed by a Bike
    [SerializeField] private float shootForce = 60f;  //How much force is applied to ball if its "shot" by a Player

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bike")
        {
            //Debug.Log(this.name + " was pushed by the bike!");
            Vector3 direction = -(collision.gameObject.transform.position - transform.position);
            pushBall(pushForce, direction);
        }
    }

    private void pushBall(float pushAmount, Vector3 direction)
    {
        rb.AddForce(pushAmount * direction);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.name == "ShootTriggerCollider")
        {
            //Debug.Log(this.name + " was shot by " + other.name);
            Vector3 direction = other.transform.forward;
            pushBall(shootForce, direction);
        }
    }
}
