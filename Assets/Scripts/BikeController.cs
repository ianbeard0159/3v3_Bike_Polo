using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeController : MonoBehaviour
{
    private Transform t;

    //All these values can be tweaked to change the feel of the movement
    [SerializeField] private float speed = 0;
    [SerializeField] public float maxSpeed = 20;
    [SerializeField] float minSpeed = 0;
    [SerializeField] public float acceleration = .06f; //How quickly spped max speed when speeding up
    [SerializeField] public float deceleration = .04f; //How quickly speed reaches 0 when slowing down
    [SerializeField] public float turnSpeed = 160; //How quickly the bike turns when pressing left/right
                                                  //Later make this into an equation based on current speed

    // Start is called before the first frame update
    void Start()
    {
        t = GetComponent<Transform>();
    }
    private Vector3 getInputDirection()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(x, 0, z);
        return direction;
    }

    public void Move(Vector3 direction)
    {
        //Turn with left/right direction
        float xVal;
        xVal = direction.x;
        t.Rotate(new Vector3(0, xVal * Time.deltaTime * turnSpeed, 0));

        //Press up or down?
        Vector3 zDirection = new Vector3(0, 0, direction.z);
        if (direction.z > 0 && speed < maxSpeed) //Speeding up with up key 
        {
            if (speed < maxSpeed) //Speed is less than max speed, speed up
            {
                speed += acceleration;
            }
            else
            {
                speed = maxSpeed; //No speeding up, already Too fast
            }
        }
        if (direction.z < 0) //Slowing down with down key 
        {
            if (speed > minSpeed) //Speed is more than the min speed, slow down
            {
                speed -= deceleration;
            }
            else
            {
                speed = minSpeed; //No slowing down, already Too slow
            }
        }
        if (direction.z == 0) //Not pressing any keys, slow down more gradually
        {
            if (speed > minSpeed)
            {
                speed -= deceleration / 2;
            }
            else
            {
                speed = 0;
            }
        }
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = getInputDirection();
        if (direction != null)
        {
            Move(direction);
        }
    }
}
