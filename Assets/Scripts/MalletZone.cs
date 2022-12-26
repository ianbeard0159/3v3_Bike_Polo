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
        zone.enabled = false;
        playerInZone = false;
    }

    //Turns on or off this Zones collider and
    //Also makes sure that ballInZone gets turned false when turning off Zones
    public void turnOffZone(bool onOff)
    {
        zone.enabled = onOff;

        if (onOff == false)
        {
            ballInZone = false;
            playerInZone = false;
        }  
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ball")
        {
            ballInZone = true; //Make sure MalletController knows the ball is within reach and
            mallet.currentZone = this; //which zone side the ball is currently in reach of
            mallet.ballRB = other.attachedRigidbody; //Grab the RB information of the ball to give to MalletController
        }

        if(other.tag == "Player")
        {
            playerInZone = true;
            mallet.currentStealablePlayer = other.GetComponentInParent<MalletController>();
            Debug.Log("Current stealable player: " + mallet.currentStealablePlayer.name);
        }
    }
}

