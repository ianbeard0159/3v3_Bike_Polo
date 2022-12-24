using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MalletLeft : MonoBehaviour
{
    public MalletController mallet;
    public bool ballInZone;
    Vector3 leftHoldSpot
    {
        get { return gameObject.transform.GetChild(0).transform.position; }
    }

    void Start()
    {
        //ballInZone = false;
        //mallet = gameObject.GetComponentInParent<MalletController>();

    }

    private void OnTriggerEnter(Collider other)
    {
        //mallet.ballRB = other.attachedRigidbody;
        ////Debug.Log(other.name + " has entered mallets left trigger collider");
        //mallet.ballOnLeft = true;
        //ballInZone = true;
    }

    private void OnTriggerStay(Collider other)
    {
    //    if (mallet.clickDown && other.tag == "Ball")
    //    {
    //        mallet.holdingBall = true;
    //        mallet.HoldBall(other);
    //        other.transform.position = leftHoldSpot;
    //    }
    }
}
