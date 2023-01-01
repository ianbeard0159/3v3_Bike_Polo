using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


//This class handles everything soecific to controlling the "mallet" and ball
//Shooting the ball, holding the ball, moving the ball to either side, etc
[RequireComponent(typeof(BikeController))]
[RequireComponent(typeof(LineRenderer))]
public class MalletController : MonoBehaviour
{

    //For testing purposes
    public bool useController = false;

    //For shooting the ball, tweakable values to change the feel
    public Vector3 currVel;     //Current velocity, set by BikeController
    [SerializeField] private float quickShotForce = 200;   //The force that is applied to the ball when shooting with the mallet
    [SerializeField] private float shootUpForce = .15f; //How much upwards force the ball receives when shot

    //For power shooting the ball
    [SerializeField] private float powerShotMin = 200;
    [SerializeField] private float powerShotMax = 400;
    [SerializeField] private float powerModifer = 1; //how much power intensifies each frame holding shoot button
    public float currentShotPower;
    public float currentLineLength; //How long the aim line is/corresponds with current shot power, also affects where the camera will look
    [SerializeField] private float maxLineLength = 5; //length that goes with max shooting power
    [SerializeField] private float minLineLength = 1; //for aiming purposes without needing to hold shoot button

    //For holding the ball with mallet
    public bool holdingBall;    //Is tje ball currently being held in the mallets hold position?
    public bool ballInZone;     //Is the ball in one of its trigger colliders?
   
    //For aiming
    public Vector3 aimDirection;
    public float minShotAngle = 10f; 
    public float maxShotAngle = 70f;

    //For Smoothing Aim
    public float aimSmoothTime = 0.15f;
    private Vector3 aimVelocity = Vector3.zero;

    Timer holdCheckTimer;
    bool donePressingButton = false;

    //Components and Stuff
    MalletZone malletLeftZone;        //Mallets area of reach on the left side
    MalletZone malletRightZone;        //Mallets area of reach on the left side
    public MalletZone currentZone;      //The zone that is currently holding the ball
    public Rigidbody ballRB;            //In order to be able to apply forces to ball
    LineRenderer aimLine;               //The line renderer that draws the aim line on screen

    void Start()
    {
        ballInZone = false;
        malletRightZone = gameObject.transform.GetChild(0).gameObject.GetComponent<MalletZone>();
        malletLeftZone = gameObject.transform.GetChild(1).gameObject.GetComponent<MalletZone>();
        aimLine = GetComponent<LineRenderer>();

        holdCheckTimer = new Timer();

        //Debug.Log("Right click or press X on controller to switch sides");
        //Debug.Log("Left click/hold or press/hold Left Bumper on controller to pick up Ball");
        //Debug.Log("Click LeftClick or Left Bumper on controller to Shoot Ball while holding it");
    }

    public void DrawAimLine(Vector3 direction, float lineLength)
    {
        if(holdingBall) //Only aim when we are holding the ball and could theoretically shoot
        {
            aimLine.enabled = true;
            aimLine.positionCount = 2;

            Vector3 shootEndPoint = currentZone.holdSpot + (direction * lineLength);
            aimLine.SetPosition(0, currentZone.holdSpot);
            aimLine.SetPosition(1, shootEndPoint);
        }
    }

    //Shoots the ball in a direction at default power
    public void ShootBall(Vector3 direction)
    {
        ballRB.isKinematic = false; //Make sure the ball isnt kinematic anymore so we shoot it with force
        direction.y = shootUpForce;
        ballRB.AddForce(currVel + (quickShotForce * direction));

        //Debug.DrawRay(currentZone.holdSpot, direction * shootForce, Color.red, 40);

        //shootVector = direction * shootForce;

        DropBall();
    }

    //Shots ball in a direction and a specific power force
    public void PowerShot(Vector3 direction, float power)
    {
        ballRB.isKinematic = false; //Make sure the ball isnt kinematic anymore so we shoot it with force
        direction.y = shootUpForce;
        ballRB.AddForce(currVel + (power * direction));
        Debug.Log("shot at power level: " + currentShotPower);
        DropBall();
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
        donePressingButton = holdCheckTimer.checkTime();

        if (holdingBall) //Controls specific to when Players are currently holding the ball
        {
            if (donePressingButton) //you didnt just begin to press the button
            {
                if (Input.GetButton("Hold/Shoot")) //holding button
                {
                    currentShotPower = Mathf.Clamp(currentShotPower + powerModifer, powerShotMin, powerShotMax);

                    //change aim line length to match current power
                    currentLineLength = ((currentShotPower - powerShotMin) / (powerShotMax - powerShotMin)) * maxLineLength;

                }
                if (Input.GetButtonUp("Hold/Shoot")) //holding ball, released the button
                {
                    
                    currentLineLength = 0;
                    if (aimDirection == Vector3.zero)
                    {
                        PowerShot(transform.forward, currentShotPower);
                        //ShootBall(transform.forward);
                    }
                    else
                    {
                        PowerShot(aimDirection, currentShotPower);
                        currentShotPower = powerShotMin;
                    }
                }
            }
            if (Input.GetButtonDown("SwitchSide")) //Pressed X or LeftClick
            {
                SwitchSides();
            }
        }
        else //controls specific to not holding ball
        {
            if (Input.GetButtonDown("Hold/Shoot")) //Not holding ball, press to pick up
            {
                if (ballInZone)
                {
                    //Debug.Log("pressed button, not holding ball, ball in zone, starting timer...");
                    HoldBall(); //attempt to hold the ball if its within zones area of reach
                    holdCheckTimer.StartTimerForSeconds(.3f); //we dont want to check for button presses for a little bit
                }
            }
        }


        ////WORKS, BUT DOESNT INCLUDE POWER SHOOTING:
        //if (Input.GetButtonDown("Hold/Shoot")) //Pressed Hold/Shoot button. Mouse: Left Click, Controller: Left Bumper
        //{
        //    if (holdingBall) //Holding the ball, Shoot!
        //    {
        //        if (aimDirection != Vector3.zero) //We're aiming, shoot ball in that direction
        //        {
        //            ShootBall(aimDirection);
        //        }
        //        else
        //            ShootBall(transform.forward); //Not aiming, just do defualt quick shot
        //    }
        //    if (!holdingBall) //Not holding ball, Try to pick up ball!
        //    {
        //        HoldBall(); //attempt to hold the ball if its within zones area of reach
        //    }
        //}

        //if (Input.GetButtonDown("SwitchSide")) //Pressed X or LeftClick
        //{
        //    if (holdingBall)
        //    {
        //        SwitchSides();
        //    }
        //}

        if (holdingBall)
        {
            Vector3 outDirection = Vector3.zero;
            if (useController) //Aiming is specific to controller, change in inspector/UI
            {
                float horizontalAim = Input.GetAxis("AimHorizontal");
                float verticalAim = Input.GetAxis("AimVertical");
                outDirection = new Vector3(Input.GetAxis("AimHorizontal"), 0, Input.GetAxis("AimVertical"));

                if (horizontalAim == 0 && verticalAim == 0)
                    aimLine.enabled = false;
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

            if(outDirection != Vector3.zero)
            {
                //Rotate the current aim inputs to match the forward of the Player
                Quaternion rotation = Quaternion.LookRotation(outDirection, Vector3.up);
                Vector3 eulerRotation = rotation.eulerAngles;
                float yRotation = eulerRotation.y;
                if (yRotation > 180)
                {
                    yRotation = -360 + yRotation;
                }
                if (currentZone.name == "RightZone")
                {
                    yRotation = Mathf.Clamp(yRotation, minShotAngle, maxShotAngle);
                }
                else
                {
                    yRotation = Mathf.Clamp(yRotation, -maxShotAngle, -minShotAngle);
                }
                eulerRotation.y = yRotation;
                //yRotation = Mathf.Clamp(yRotation, 0 + 360, 90 + 360);
                Quaternion clampedRotation = Quaternion.Euler(eulerRotation);

                aimDirection = Vector3.SmoothDamp(aimDirection, (clampedRotation * transform.forward).normalized, ref aimVelocity, aimSmoothTime);

                if (aimDirection != Vector3.zero)
                {
                    currentLineLength = Mathf.Clamp(currentLineLength, minLineLength, maxLineLength);
                    DrawAimLine(aimDirection, currentLineLength);
                }
            }
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
