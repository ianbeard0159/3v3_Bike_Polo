using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MalletController : MonoBehaviour
{

    public Rigidbody ballRB;
    public float shootForce;

    public bool holdingBall;
    public bool ballInZone;

    public Vector3 currVel;

    Collider malletLeftZone;
    Collider malletRightZone;
    public MalletZone currentZone;

    // Start is called before the first frame update
    void Start()
    {
        ballInZone = false;
        malletRightZone = gameObject.transform.GetChild(0).gameObject.GetComponent<Collider>();
        malletLeftZone = gameObject.transform.GetChild(1).gameObject.GetComponent<Collider>();
        turnOnOffMallet(false);
    }

    public void turnOnOffMallet(bool onOff)
    {
        malletLeftZone.enabled = onOff;
        malletRightZone.enabled = onOff;

        if(onOff == false)
            ballInZone = false;

    }

    public void shootBall(Vector3 direction)
    {
        ballRB.AddForce(currVel + (shootForce * direction));
        holdingBall = false;
    }

    private void switchSides()
    {

    }

    public void HoldBall()
    {
        if (ballInZone)
        {
            holdingBall = true;
            ballRB.velocity = Vector3.zero;
            ballRB.transform.position = currentZone.holdSpot;
        }
    }
}
