using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


//This class handles everything soecific to controlling the "mallet" and ball
//Shooting the ball, holding the ball, moving the ball to either side, etc
public class MalletController : MonoBehaviour
{

    public Rigidbody ballRB;
    public float shootForce;    //The force that is applied to the ball when shooting with the mallet

    public bool holdingBall;    //Is tje ball currently being held in the mallets hold position?
    public bool ballInZone;     //Is the ball in one of its trigger colliders?

    public Vector3 currVel;     //Current velocity, set by BikeController

    MalletZone malletLeftZone;        //Mallets area of reach on the left side
    MalletZone malletRightZone;        //Mallets area of reach on the left side
    public MalletZone currentZone;      //The zone that is currently holding the ball



    void Start()
    {
        ballInZone = false;
        malletRightZone = gameObject.transform.GetChild(0).gameObject.GetComponent<MalletZone>();
        malletLeftZone = gameObject.transform.GetChild(1).gameObject.GetComponent<MalletZone>();
        turnOnOffZones(false);


    }

    //Disables the trigger colliders of its zones when passed false, enables them when passed on
    //Also makes sure that ballInZone gets turned false when turning off Zones
    public void turnOnOffZones(bool onOff)
    {
        malletLeftZone.turnOffZone(onOff);
        malletRightZone.turnOffZone(onOff);

        if(onOff == false)
            ballInZone = false;
    }

    //Shoots the ball in a direction
    //TODO pass a direction from inputs instead of just transform.forward
    public void shootBall(Vector3 direction)
    {

        ballRB.isKinematic = false; //Male sure the ball isnt kinematic anymore so we shoot it with force
        ballRB.AddForce(currVel + (shootForce * direction));
        currentZone = null; //Ball no longer in a zone
        holdingBall = false; //Not holding ball
    }

    private void SwitchSides()
    {
        //Make sure switching sides is viable, must be holding the ball in a pocket
        if (holdingBall)
        {
            if (currentZone.name == "RightZone") //Ball is in the right pocket
            {
                ballRB.transform.position = malletLeftZone.holdSpot; //Move it to the other pocket
                currentZone = malletLeftZone; //update current zone
            }
            else  //Ball must be in the left pocket
            {
                ballRB.transform.position = malletRightZone.holdSpot; //Move it to the other pocket
                currentZone = malletRightZone;  //update current zone
            }
        }
    }

    //Holds the ball in one of its zones and manually makes sure ball's rigidbody can't move/get forces applied
    public void HoldBall()
    {
        if (currentZone != null)
        {
            holdingBall = true;
            ballRB.isKinematic = true; //Make the ball kinematic while its being held
            ballRB.gameObject.transform.position = currentZone.holdSpot;
        }
    }

    //TODO
    public void StealBall()
    {

    }

    //TODO
    public void PassBall()
    {

    }

    //Getting inputs specific to times when Player is currently holding the ball
    private void GetHoldingBallInputs()
    {
        //if (Input.GetButtonUp("Hold/Shoot")) //Let go of A/LeftClick
        //{
        //    ballRB.isKinematic = false; //Make sure the ball isnt kinematic anymore before we shoot it
        //    shootBall(transform.forward); //we can shoot it
        //    holdingBall = false; //no longer holding ball
        //    turnOnOffZones(false); //Turn off zones whether we were shooting or not
        //}

        //if (Input.GetButtonDown("SwitchSide")) //Pressed X or LeftClick
        //{
        //    SwitchSides();
        //}
    }

    //Get inputs from player to control things related to "Mallet use" or manipulating the ball
    private void getMalletInputs()
    {
        if (holdingBall)
        {

        }

        if (!holdingBall)
        {

        }

        if (Input.GetButton("Hold/Shoot"))  //Pressing A/LeftClick
        {
            turnOnOffZones(true); //turn on mallet zones to search for the ball
            HoldBall(); //attempt to hold the ball if its within zones area of reach
            
        }
        //if (Input.GetButtonUp("Hold/Shoot")) //Let go of A/LeftClick
        //{
        //    turnOnOffZones(false); //Turn off zones whether we were shooting or not
        //}

        if (Input.GetButtonUp("Hold/Shoot")) //Let go of A/LeftClick
        {
            if (holdingBall) //If you were holding the ball, 
            {
                shootBall(Camera.main.transform.rotation * Vector3.forward); //we can shoot it
                holdingBall = false; //no longer holding it
            }

            turnOnOffZones(false); //Turn off zones whether we were shooting or not
        }

        if (Input.GetButtonDown("SwitchSide")) //Pressed X or LeftClick
        {
            if (holdingBall)
            {
                SwitchSides();
            }
        }
    }


    private void Update()
    {
        getMalletInputs();

        //if (holdingBall)
        //{
        //    GetHoldingBallInputs();
        //}
        
        
    }
}
