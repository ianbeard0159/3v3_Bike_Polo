using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Receive information from its collider in order to send it to Mallet
public class MalletZone : MonoBehaviour
{
    public MalletController mallet;
    Collider zone;

    public Vector3 holdSpot
    {
        get { return gameObject.transform.GetChild(0).transform.position; }
    }

    public string zoneSide;

    void Start()
    {

        mallet = gameObject.GetComponentInParent<MalletController>();
        zone = GetComponent<Collider>();
        zoneSide = this.name;
    }

    public void turnOffZone()
    {
        zone.enabled = false;
        mallet.ballInZone = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Ball")
        {
            mallet.currentZone = this;
            mallet.ballInZone = true;
            mallet.ballRB = other.attachedRigidbody;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Ball")
            mallet.ballInZone = true;

    }


}
