using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class just receives information from its collider in order to send it to MalletController
//And holds the definition for its own holdSpot.
//MalletZoneLeft and MalletZoneRight have their own holdspots, each exist within MalletController
public class MalletZone : MonoBehaviour
{
    public MalletController mallet;

    public bool ballInZone;
    public bool playerInZone;

    Collider zone;

    public Vector3 holdSpot    //The position of this Zones "hold spot/pocket"
    {
        get { return gameObject.transform.GetChild(0).transform.position; }
    }


    void Start()
    {
        mallet = gameObject.GetComponentInParent<MalletController>();
        zone = GetComponent<Collider>();
        //zone.enabled = false;
        playerInZone = false;
    }

    public void enableZone(bool onOff)
    {
        zone.enabled = onOff;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Ball")
        {
            mallet.ballInZone = false;
           // ballInZone = false; //Make sure MalletController knows the ball is within reach and
            mallet.currentZone = null; //which zone side the ball is currently in reach of
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Ball")
        {
            mallet.ballInZone = true;
            //ballInZone = true; //Make sure MalletController knows the ball is within reach and
            mallet.currentZone = this; //which zone side the ball is currently in reach of
            mallet.ballRB = other.attachedRigidbody; //Grab the RB information of the ball to give to MalletController
        }
    }
}

