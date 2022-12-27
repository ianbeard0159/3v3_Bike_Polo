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
    public float shootUpForce = .15f;
    public float aimLineLength = 5;

    public bool holdingBall;    //Is tje ball currently being held in the mallets hold position?
    public bool ballInZone;     //Is the ball in one of its trigger colliders?

    public Vector3 currVel;     //Current velocity, set by BikeController

    MalletZone malletLeftZone;        //Mallets area of reach on the left side
    MalletZone malletRightZone;        //Mallets area of reach on the left side
    public MalletZone currentZone;      //The zone that is currently holding the ball


    public MalletController currentStealablePlayer;

    public Vector3 aimDirection;
   // public Vector3 rotatedAimDirection;
    LineRenderer aimLine;

    public bool useController = true;
    private Vector3 aimVelocity = Vector3.zero;
    public float minShotAngle = 10f;
    public float maxShotAngle = 70f;
    public float aimSmoothTime = 0.15f;


    void Start()
    {
        ballInZone = false;
        malletRightZone = gameObject.transform.GetChild(0).gameObject.GetComponent<MalletZone>();
        malletLeftZone = gameObject.transform.GetChild(1).gameObject.GetComponent<MalletZone>();
        aimLine = GetComponent<LineRenderer>();

        //Debug.Log("Right click or press X on controller to switch sides");
        //Debug.Log("Left click/hold or press/hold Left Bumper on controller to pick up Ball");
        //Debug.Log("Click LeftClick or Left Bumper on controller to Shoot Ball while holding it");
    }

    public void DrawAimLine(Vector3 direction)
    {
        if(holdingBall) //Only aim when we are holding the ball and could theoretically shoot
        {
            aimLine.enabled = true;
            aimLine.positionCount = 2;

            Vector3 shootEndPoint = currentZone.holdSpot + (direction * aimLineLength);
            aimLine.SetPosition(0, currentZone.holdSpot);
            aimLine.SetPosition(1, shootEndPoint);
        }
    }

    //Shoots the ball in a direction
    public void ShootBall(Vector3 direction)
    {
        ballRB.isKinematic = false; //Make sure the ball isnt kinematic anymore so we shoot it with force
        direction.y = shootUpForce;
        ballRB.AddForce(currVel + (shootForce * direction));

        //Debug.DrawRay(currentZone.holdSpot, direction * shootForce, Color.red, 40);

        //shootVector = direction * shootForce;

        DropBall();
    }

    public void QuickShoot()
    {
        ShootBall(transform.forward);
    }

    //Holds the ball in one of its zones and manually makes sure ball's rigidbody can't move/get forces applied
    public void HoldBall()
    {
        if (ballInZone && currentZone != null)
        {
            holdingBall = true;
            ballRB.isKinematic = true;
            ballRB.transform.position = currentZone.holdSpot;
        }
    }

    //Set booleans and whatnot for whenever player lets go of ball
    public void DropBall()
    {
        holdingBall = false; //Not holding ball
        ballRB.isKinematic = false;
        currentZone = null; //Ball no longer in a zone
       
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

    //TODO
    public void StealBall()
    {

    }

    //TODO
    public void PassBall()
    {

    }

    //Get inputs from player to control things related to "Mallet use" or manipulating the ball
    private void getMalletInputs()
    {

        if (Input.GetButtonDown("Hold/Shoot")) //Pressed Hold/Shoot button. Mouse: Left Click, Controller: Left Bumper
        {
            if (holdingBall) //Holding the ball, Shoot!
            {
                if (aimDirection != Vector3.zero) //We're aiming, shoot ball in that direction
                {
                    ShootBall(aimDirection);
                }
                else
                    QuickShoot(); //Not aiming, just do defualt quick shot
            }
            if (!holdingBall) //Not holding ball, Try to pick up ball!
            {
                HoldBall(); //attempt to hold the ball if its within zones area of reach
            }
        }

        if (Input.GetButtonDown("SwitchSide")) //Pressed X or LeftClick
        {
            if (holdingBall)
            {
                SwitchSides();
            }
        }

        if (holdingBall)
        {
            Vector3 outDirection = Vector3.zero;
            if (useController) //Aiming is specific to controller, change in inspector/UI
            {
                float horizontalAim = Input.GetAxis("AimHorizontal");
                float verticalAim = Input.GetAxis("AimVertical");
                outDirection = new Vector3(Input.GetAxis("AimHorizontal"), 0, Input.GetAxis("AimVertical"));

                if (horizontalAim != 0 || verticalAim != 0)
                {

                    if (horizontalAim != 0)
                    {
                        outDirection.x = horizontalAim;
                    }

                    if (verticalAim != 0)
                    {
                        outDirection.z = verticalAim;
                    }

                    //DrawAimLine(aimDirection, Color.white);
                    //aimDirection = rotatedAimDirection.normalized;
                }
            }
            else if (!useController)
            {
                //Get the center of the screen, the position of the mouse
                //Calculate that direction, and then rotate it to match Player
                Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
                outDirection = (Input.mousePosition - screenCenter).normalized;
                outDirection.z = outDirection.y / 2;
                outDirection.y = 0;
            }
            //Rotate the current aim inputs to match the forward of the Player
            Quaternion rotation = Quaternion.LookRotation(outDirection, Vector3.up);
            Vector3 eulerRotation = rotation.eulerAngles;
            float yRotation = eulerRotation.y;
            if (yRotation > 180) {
                yRotation = -360 + yRotation;
            }
            if (currentZone.name == "RightZone") {
                yRotation = Mathf.Clamp(yRotation, minShotAngle, maxShotAngle);
            }
            else {
                yRotation = Mathf.Clamp(yRotation, -maxShotAngle, -minShotAngle);
            }
            eulerRotation.y = yRotation;
            Debug.Log(eulerRotation.y);
            //yRotation = Mathf.Clamp(yRotation, 0 + 360, 90 + 360);
            Quaternion clampedRotation = Quaternion.Euler(eulerRotation);

            aimDirection = Vector3.SmoothDamp(aimDirection, (clampedRotation * transform.forward).normalized, ref aimVelocity, aimSmoothTime);

            DrawAimLine(aimDirection);
        }
        if (!holdingBall || aimDirection == Vector3.zero)
        {
            aimLine.enabled = false;
            aimLine.positionCount = 0;
            aimDirection = Vector3.zero;
        }
    }

    private void Update()
    {
        getMalletInputs();

        if (holdingBall)
        {
            HoldBall();
        }
    }
}
