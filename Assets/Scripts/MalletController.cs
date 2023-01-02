using UnityEngine;


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

    //For discerning the difference between holding button and simply pressing button
    Timer buttonPressedTimer;

    //Components and Stuff
    MalletZone malletLeftZone;        //Mallets area of reach on the left side
    MalletZone malletRightZone;        //Mallets area of reach on the left side
    public MalletZone currentZone;      //The zone that is currently holding the ball
    public Rigidbody ballRB;            //In order to be able to apply forces to ball
    LineRenderer aimLine;               //The line renderer that draws the aim line on screen

    private void Awake()
    {
        malletRightZone = gameObject.transform.GetChild(0).gameObject.GetComponent<MalletZone>();
        malletLeftZone = gameObject.transform.GetChild(1).gameObject.GetComponent<MalletZone>();

        aimLine = GetComponent<LineRenderer>();
    }
    void Start()
    {
        ballInZone = false;
        DisableLine();
        buttonPressedTimer = new Timer();
        //Debug.Log("Right click or press X on controller to switch sides");
        //Debug.Log("Left click/hold or press/hold Left Bumper on controller to pick up Ball");
        //Debug.Log("Click LeftClick or Left Bumper on controller to Shoot Ball while holding it");
    }

    public void enableZones(bool onOff)
    {
        malletLeftZone.enableZone(onOff);
        malletRightZone.enableZone(onOff);
    }

    public void DrawAimLine(Vector3 direction, float lineLength)
    {
        if(holdingBall) //Only aim when we are holding the ball and could theoretically shoot
        {
            aimLine.enabled = true;
            aimLine.positionCount = 2;

            currentLineLength = Mathf.Clamp(currentLineLength, minLineLength, maxLineLength);

            Vector3 shootEndPoint = currentZone.holdSpot + (direction * lineLength);
            aimLine.SetPosition(0, currentZone.holdSpot);
            aimLine.SetPosition(1, shootEndPoint);
        }
        else
        {
            DisableLine();
        }
    }

    public void DisableLine()
    {
        aimLine.enabled = false;
        aimLine.positionCount = 0;
        aimDirection = Vector3.zero;
        currentLineLength = 0;

    }
    /*
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
    */

    //Shots ball in a direction and a specific power force
    public void ShootBall(Vector3 direction, float power)
    {
        ballRB.isKinematic = false; //Make sure the ball isnt kinematic anymore so we shoot it with force
        direction.y = shootUpForce;

        ballRB.AddForce(currVel + (power * direction));
        Debug.Log("shot at power level: " + currentShotPower);

        currentShotPower = powerShotMin; //reset shot power when down
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
        DisableLine();
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

    //Aim in a direction and draw a line indicator
    //Makes sure the direction is relative to bike and is within angle limitation vars
    public void AimRelativeToBike(Vector3 outDirection)
    {
        //Rotate the given direction to match the forward of the Player
        Quaternion rotation = Quaternion.LookRotation(outDirection, Vector3.up);
        Vector3 eulerRotation = rotation.eulerAngles;
        float yRotation = eulerRotation.y;

        //if (yRotation > 180)
        //{
        //    yRotation = -360 + yRotation;
        //}
        if (currentZone.name == "RightZone") //Our deadzones are different depending on which zone we're aiming from (Left, Right)
        {
            //Make sure the angle is not within our deadzone 
            //deadzone is anything outside min and max
            yRotation = HelperFunctions.reverseClamp(yRotation, maxShotAngle, 360f + minShotAngle);

            //yRotation = Mathf.Clamp(yRotation, minShotAngle, maxShotAngle);
        }
        else
        {
            //Make sure the angle is not within our deadzone 
            //ensure aim direction is valid when ball is in the left
            yRotation = HelperFunctions.reverseClamp(yRotation, -minShotAngle, 360f - maxShotAngle);

            //yRotation = Mathf.Clamp(yRotation, -maxShotAngle, -minShotAngle);
        }
        eulerRotation.y = yRotation;
        //yRotation = Mathf.Clamp(yRotation, 0 + 360, 90 + 360);
        Quaternion clampedRotation = Quaternion.Euler(eulerRotation);

        //Smooth and rotate current aimDirection towards our forward
        aimDirection = Vector3.SmoothDamp(aimDirection, (clampedRotation * transform.forward).normalized, ref aimVelocity, aimSmoothTime);

        if (aimDirection != Vector3.zero)
        {
            currentLineLength = Mathf.Clamp(currentLineLength, minLineLength, maxLineLength);
            DrawAimLine(aimDirection, currentLineLength);
        }
    }

    //Get inputs from player to control things related to "Mallet use" or manipulating the ball
    private void getMalletInputs()
    {
        //For Controls specific to holding the ball: Aiming, Shooting, Switching Hold Side
        if (holdingBall)
        {
            Vector3 outDirection = Vector3.zero;
            //For aiming:
                //Aiming is specific to controller, change in inspector/UI
            if (useController)      
            {
                float horizontalAim = Input.GetAxis("AimHorizontal");
                float verticalAim = Input.GetAxis("AimVertical");
                outDirection = new Vector3(Input.GetAxis("AimHorizontal"), 0, Input.GetAxis("AimVertical"));

                if (horizontalAim == 0 && verticalAim == 0)
                    aimLine.enabled = false;
            }
                //Aiming for keyboard/mouse
            else if (!useController)
            {
                //Get the center of the screen, the position of the mouse
                //Calculate that direction, and then rotate it to match Player
                Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
                outDirection = (Input.mousePosition - screenCenter).normalized;
                outDirection.z = outDirection.y / 2;
                outDirection.y = 0;
            }
            //For rotating and validating aim
            if (outDirection != Vector3.zero)
            {
                AimRelativeToBike(outDirection);
            }

            //For shooting
            if (buttonPressedTimer.checkTime()) //you didnt just press the button recently 
            {
                if (Input.GetButton("Hold/Shoot")) //holding button
                {
                    //Charge the shot power while holding the buttpn
                    currentShotPower = Mathf.Clamp(currentShotPower + powerModifer, powerShotMin, powerShotMax);

                    //Change aim line length to match current power
                    currentLineLength = ((currentShotPower - powerShotMin) / (powerShotMax - powerShotMin)) * maxLineLength;
                }
                if (Input.GetButtonUp("Hold/Shoot")) //holding ball, released the button
                {
                    if (aimDirection == Vector3.zero)
                    {
                        ShootBall(transform.forward, powerShotMin); //if not aiming, just do a quick shot forward with min strength
                    }
                    else
                    {
                        ShootBall(aimDirection, currentShotPower); //The longer the button was pressed, the more power
                    }
                }
            }
            //For siwtching which side ball is on
            if (Input.GetButtonDown("SwitchSide")) //Pressed X or LeftClick
            {
                SwitchSides();
            }
        }

        //For controls while not holding ball: Picking up ball
        else
        {
            if (Input.GetButtonDown("Hold/Shoot")) //Not holding ball, press to pick up
            {
                if (ballInZone)
                {
                    HoldBall(); //attempt to hold the ball if its within zones area of reach
                    buttonPressedTimer.StartTimerForSeconds(.3f); //we dont want to check for button presses for a little bit
                }
            }
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
