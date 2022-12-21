using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField] private float pushForce = 1f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bike")
        {
            Debug.Log(this.name + " was hit by the bike!");
            Vector3 direction = collision.gameObject.transform.position - transform.position;
            rb.AddForce(pushForce * -direction);
        }
    }
}
