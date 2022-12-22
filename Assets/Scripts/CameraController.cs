using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float distance = -2f;
    [SerializeField] public Vector3 back;

    // Update is called once per frame
    void Update()
    {
        back = -target.transform.forward;
        back.y = 0.4f;
        transform.position = target.transform.position - back * distance;
        transform.forward = target.transform.position - transform.position;
    }
}
