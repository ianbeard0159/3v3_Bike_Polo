using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MalletController : MonoBehaviour
{

    Rigidbody ballRB;
    public float shootForce;
    public bool clickDown;
    public bool holdingBall;
    Vector3 holdSpot
    {
        get { return gameObject.transform.GetChild(1).transform.position; }
    }
    public Vector3 currVel;

    Collider malletArea;

    // Start is called before the first frame update
    void Start()
    {
        //holdSpot = gameObject.transform.GetChild(1).transform.position;
        clickDown = false;
        malletArea = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void turnOnOffMallet(bool onOff)
    {
        malletArea.enabled = onOff;
    }

    public void shootBall(Vector3 direction, Vector3 velocity)
    {
        if(ballRB != null && clickDown)
        {
            //Debug.Log("my direction is:" + transform.forward);
            //Debug.Log("my Bikes direction is:" + transform.parent.forward);

            ballRB.AddForce(velocity + (shootForce * direction));
            //Debug.DrawRay(ballRB.position, transform.forward, Color.red);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //if(other.name == "Ball" && holdBall)
        //{
        //    ballRB = other.attachedRigidbody;
            
        //    Debug.Log("malletes trigger enter");
        //}
       
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.name == "Ball" && clickDown)
        {
            HoldBall(other);
            //holdingBall = true;
            ////holdSpot = gameObject.transform.GetChild(1).transform.position;
            //other.transform.position = holdSpot;

            //ballRB = other.attachedRigidbody;
            //ballRB.velocity = Vector3.zero;
            //ballRB.position = holdSpot;
            //shootBall(transform.forward, currVel);
        }
    }
    private void switchSides()
    {

    }

    private void HoldBall(Collider ball)
    {
        holdingBall = true;
        //holdSpot = gameObject.transform.GetChild(1).transform.position;
        ball.transform.position = holdSpot;

        ballRB = ball.attachedRigidbody;
        ballRB.velocity = Vector3.zero;
        ballRB.position = holdSpot;
        shootBall(transform.forward, currVel);
    }
}
