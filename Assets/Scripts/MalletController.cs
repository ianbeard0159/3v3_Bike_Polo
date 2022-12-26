using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//This class handles everything soecific to controlling the "mallet" and ball
//Shooting the ball, holding the ball, moving the ball to either side, etc
public class MalletController : MonoBehaviour
{

    public Rigidbody ballRB;
    public float shootForce;    //The force that is applied to the ball when shooting with the mallet

    public bool holdingBall;    //Is tje ball currently being held in the mallets hold position?
    //public bool ballInZone;     //Is the ball in one of its trigger colliders?

    public Vector3 currVel;     //Current velocity, set by BikeController

    MalletZone malletLeftZone;        //Mallets area of reach on the left side
    MalletZone malletRightZone;        //Mallets area of reach on the left side
    public MalletZone currentZone;      //The zone that is currently holding the ball

    public MalletController currentStealablePlayer; //Another MalleteController that is on the steal range
    //MalletController currentPassablePlayer;

    void Start()
    {
        malletRightZone = gameObject.transform.GetChild(0).gameObject.GetComponent<MalletZone>();
        malletLeftZone = gameObject.transform.GetChild(1).gameObject.GetComponent<MalletZone>();

        Debug.Log("Right click or press X on controller to switch sides");
        Debug.Log("Left click/hold or press/hold A on controller to pick up Ball");
        Debug.Log("Let go of LeftClick or A on controller to Shoot Ball while holding it");
    }

    //Disables the trigger colliders of its zones when passed false, enables them when passed on
    //Pass in a String matching the side needing to be turn off/on, "Left", "Right", or "Both"
    public void turnOnOffZones(bool onOff, string side)
    {
        switch (side)
        {
            case "Left":
                malletLeftZone.turnOffZone(onOff);
                break;
            case "Right":
                malletRightZone.turnOffZone(onOff);
                break;
            case "Both":
                malletLeftZone.turnOffZone(onOff);
                malletRightZone.turnOffZone(onOff);
                break;
        }
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
        if(currentZone != null)
        {
            if (currentZone.ballInZone)
            {
                holdingBall = true;
                ballRB.isKinematic = true; //Make the ball kinematic while its being held
                ballRB.gameObject.transform.position = currentZone.holdSpot; //Put the ball in the current zones hold spot
            }
        }
    }


    //TODOS:
    //each players controller has to know when a ball was stolen from them OR if they can steal the ball from amother player and then be able to steal
    //In order to prevent being able to continue to hold the ball even after ball is stolen

    //TODO
    public void BeStolenFrom()
    {
        //Pseudo code logic
        //if (holdingBall && !currentStealablePlayer.holdingBall) //Im holding the ball, they're not
        //{
        //    if(currentStealablePlayer != null)
        //    {
        //        holdingBall = false;
        //    }
        //}
        
    }

    //TODO
    public void StealBall()
    {
        //Pseudo code logic
        //if (!holdingBall) //I'm not holding the ball
        //{
        //    if (currentStealablePlayer.holdingBall) //They are
        //    {
        //        if (currentStealablePlayer != null)
        //        {
        //            HoldBall(); //Hold the ball
        //            currentStablePlayer.BeStolenFrom();
        //        }
        //    }
        //}
    }

    //TODO
    public void PassBall()
    {

    }

    //Get inputs from player to control things related to "Mallet use" or manipulating the ball
    private void getMalletInputs()
    {
        if (Input.GetButton("Hold/Shoot"))  //While Pressing A/LeftClick
        {
            turnOnOffZones(true, "Both"); //turn on mallet zones to search for the ball
            HoldBall(); //attempt to hold the ball if its within zones area of reach
        }

        if (Input.GetButtonUp("Hold/Shoot")) //Let go of A/LeftClick
        {
            if (holdingBall) //If you were holding the ball, 
            {
                shootBall(transform.forward); //we can shoot it
                holdingBall = false; //no longer holding it
            }

            turnOnOffZones(false, "Both"); //Turn off zones whether we were shooting or not
        }

        if (Input.GetButtonDown("SwitchSide")) //Pressed X or LeftClick
        {
            if (holdingBall)
            {
                SwitchSides();
            }
        }

        //if (Input.GetButtonDown("Steal")) //Pressed B or mouse2 on to steal
        //{
        //    StealBall();
        //}
    }

    private void Update()
    {
        getMalletInputs();

    }
}
