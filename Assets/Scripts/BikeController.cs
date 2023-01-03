using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(MalletController))]
public class BikeController : MonoBehaviour
{
    //Maximum value for balance (either direction); measured as 0 to 100
    private const float MAX_BALANCE_LIMIT = 100;
    //Maximum number of degrees the bike can rotate before being considered "fallen over"
    private const float MAX_BALANCE_ROTATION = 30;
    //Degrees to rotate the bike when it has fallen over
    private const float FALLEN_OVER_ROTATION = 75;

    //Values can be tweaked to change the feel of the Bike Movement:
    //Tweaks how the dash works:
    [SerializeField] public float dashingSpeedModifier = 2.5f; //The modifer that sets the Dash speed based on maxSpeed*dashSpeedMod
    [SerializeField] private float dashDuration = .5f; //how long the dash lasts in seconds
    [SerializeField] private float dashCooldown = 5; //how long the cooldown is between dashes
    [SerializeField] private float dashTurnSpeed = 50; //how fast bike turns only when dashing

    //Changes the speed limits of the bike
    [SerializeField] private float maxSpeed = 20;
    [SerializeField] private float minSpeed = 0;

    //Changes how quickly bike speeds up, slows down
    [SerializeField] private float acceleration = .06f; //How quickly to reach max speed when speeding up
    [SerializeField] private float deceleration = .04f; //How quickly yo reach reache min speed when slowing down

    //Changes how quickly bike turns
    //[SerializeField] private float turnSpeedModifer = .3f; //Current speed affects how quickly you turn
    [SerializeField] private float turnSpeed = 160; //How quickly the bike turns when pressing left/right
    [SerializeField] private float maxTurnSpeed = 180;  //maxTurnSpeed is hit when speed is 0
    [SerializeField] private float minTurnSpeed = 20;    //maxTurnSpeed is hit when speed is at max

    [SerializeField] private float pushForce = 40f; //Changes how much force is applied to the ball when collided with

    //The current speed the bike is going
    private float speed = 0;
    public Vector3 currVel //Only used to give to Mallet so shots stay accurate
    {
        get { return transform.forward * speed; }
    }

    //Changes what/how balance is related to other factors
    [SerializeField] public float turningBalanceModifier = 0.1f; //How much your turn speed affects the rate of balance shifting
    [Range(0, 100)] private float balanceDangerThreshold = 90;  // Balance value that player becomes at risk of "falling over"

    //Balance variables 
    public float currBalance = 0; // The current balance value of the bike. Tilting to the right is positive, the left is negative
    private bool isInBalanceDangerZone = false; // Boolean which returns if the player's balance is in the "balance danger zone," meaning the player is at risk of falling over
    public float dangerZoneTimeLimit = 2f; // How long the player can be in the "balance danger zone" before falling over

    //Booleans used for animation, logic, etc
    public bool isDashing;

    // I've fallen, and I can't get up!
    public bool isFallen;

    //Neccessary components
    private Rigidbody rb;
    private MalletController mallet;
    private Animator animationController;
    private MalletZone currentZone;
    private Transform followTarget;
    private Transform bikeModel;
    private Vector3 lastFollowTargetPos = Vector3.zero;

    //Inputs
    private Vector3 inputDir;

    //Helper timers
    private Timer dashDurationTimer; //Timer to determine when to stop a dash
    private Timer dashCooldownTimer; //Timer to determine if you cant dash again after dashing
    private Timer balanceDangerTimer; //Timer to determine if player has stayed in the "balance danger zone" for too long

    private List<string> holdingStates = new List<string>() {
        "BallHeldLeft",
        "BallHeldRight"
    };

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = Vector3.zero;
        rb.inertiaTensorRotation = new Quaternion(0, 0, 0, 1);
        mallet = GetComponent<MalletController>();
        bikeModel = gameObject.transform.GetChild(3).gameObject.GetComponent<Transform>();
    }

    // Start is called before the first frame update
    void Start()
    {
        isDashing = false;

        dashDurationTimer = new Timer();
        dashCooldownTimer = new Timer();
        balanceDangerTimer = new Timer();

        animationController = GetComponent<Animator>();
        setHoldingState("Normal");
        followTarget = gameObject.transform.Find("Follow Target").transform;   // <- NULLREF ERROR

        //Debug.Log("Keyboard/Mouse: Dash on Shift, Hold/Shoot on Left Click, Switch Sides on Right Click");
        //Debug.Log("Keyboard/Mouse: WASD to Drive/Turn.");
        //Debug.Log("(Xbox) Controller: Triggers to Drive, Left Stick to Turn");
        //Debug.Log("(Xbox) Controller: Dash on X, Hold/Shoot on Left Bumper, Switch Sides on X (holding ball");
    }

    //Gets any Button Inputs related to movement of bike
    public void GetButtonInputs()
    {
        //Temporary solution to falling over purely for testing purposes, puts the GetUp() function on a button press
        if (Input.GetButtonDown("GetUp"))
        {
            GetUp();
        }

        //Check for input,
        //check to make sure we're not already dashing,
        //check that we're not holding ball,
        //check that the dash is off Cooldown
        //check to see Player is going a reasonable speed (not stopped/slow) before we can dash
        if (Input.GetButtonDown("Dash") && !isDashing && !mallet.holdingBall && dashCooldownTimer.checkTime() && speed >= (maxSpeed / 2))
        {
            Dash();
        }
    }
    //Gets any movement inputs, updates the direction of the bike based on those inputs
    private Vector3 GetInputDirection()
    {
        float turnAmount = Input.GetAxis("Turn"); //Left/Right keys, A/D keys, or Left joystick on controller
        float driveAmount = Input.GetAxis("Drive"); //Down/Up, S/W or Left/Right trigger on controller

        Vector3 direction = new Vector3(turnAmount, 0, driveAmount);
        return direction;
    }

    //Moves and rotates the bike based on given direction
    public void MovePositioRB(Vector3 direction)
    {
        Quaternion turn = Quaternion.Euler(0, inputDir.x * Time.fixedDeltaTime * turnSpeed, 0);

        rb.MoveRotation(rb.rotation * turn);
        rb.MovePosition(rb.position + (transform.forward * speed * Time.fixedDeltaTime));
    }

    private void setHoldingState(string in_param) {
        // If the input parameter is not within the list, then
        //    all parameters are set to false, resulting in the 
        //    animator being set to the "normal" holding state
        foreach (string key in holdingStates) {
            if (key == in_param) {
                animationController.SetBool(key, true);
            }
            else {
                animationController.SetBool(key, false);
            }
        }
    }

    private void updateHoldingState()
    {
        // Only change the camera if the ball is 
        //    actually being held

        if (!mallet.holdingBall)
        {
            setHoldingState("Normal");
            return;
        }

        currentZone = mallet.currentZone;
        switch (currentZone?.name)
        {
            case "RightZone":
                setHoldingState("BallHeldRight");
                break;
            case "LeftZone":
                setHoldingState("BallHeldLeft");
                break;
            default:
                setHoldingState("Normal");
                break;
        }
    }

    //Collision Enters
    //Hitting Ball: Pushes the ball with a force defined by pushForce if bike collides with it
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            Vector3 direction = (collision.gameObject.transform.position - transform.position);
            collision.rigidbody.AddForce(pushForce * direction);
        }
    }

    //Collision Stays:
    //Walls: abort the dash (fixes bugs with going through wall when dashing)
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
        {

            //If we're dashing and touch a wall, we have to stop dashing. Temporary cuz of bugs where the bike would dash through walls
            if (isDashing)
            {
                //Debug.Log("Hit a wall, no dash for you");
                isDashing = false;
                dashCooldownTimer.StartTimerForSeconds(dashDuration);
            }
        }
    }

    //Calculates and sets what the current speed is
    public void CalculateSpeed(float zInput)
    {
        if (isDashing)
        {

        }
        if (isFallen)
        {

        }

        //Press up or down?
        if (zInput > 0 && speed < maxSpeed) //Speeding up with up key up right trigger
                speed += acceleration;
        if (zInput < 0) //Slowing down with down key or right trigger, maybe add reverse here
                speed -= deceleration;

        //Not pressing any keys, slow down more gradually to a halt
        if (zInput == 0) 
        {
            if (speed > 0)
                speed -= deceleration / 2;
            else
                speed = 0;
        }

        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
        mallet.currVel = currVel; //make sure mallet knows what velocity we're going so shooting stay accurate
    }

    //Calculates and sets the current turn speed based on speed
    //and if Player is Dashing or not
    public void CalculateTurnSpeed()
    {
        float absSpeed = Mathf.Abs(speed);

        //Linear Relation. Going faster -> turning slower
        turnSpeed = (1 - (absSpeed / maxSpeed)) * maxTurnSpeed;

        //Curved relation, Going faster -> turning faster
        //turnSpeed = (speed * speed) * turnSpeedModifer + 20;

        //Curved relation, Going faster -> turning slower
        //if (speed == 0)
        //turnSpeed = maxTurnSpeed;
        // else
        //{
        //turnSpeed = (1 / absSpeed) * 2000; //2000 is temp standin for modifier
        // }

        turnSpeed = Mathf.Clamp(turnSpeed, minTurnSpeed, maxTurnSpeed);
    }

    //Calculates the shift in balance, or the "rate of change" of how quikcly the balance is shifting away from 0
    //Depends on factors such as turnSpeed
    //More modifiers/constants/etc can go in here to alter how quickly a player falls off balance or regains balance
    //Options: dashing could throw you off balance a flat amount in whichever way youre turning,
    //if youre moving at max speed you could regain balance faster, or lose it slower
    //if youre holding the ball, you could lose balance quicker or regain balance slower
    public void CalculateBalanceShift()
    {
        float balanceShiftRate = 0;

        //This affects how quickly you lose balance:

        //Balance is affected by how much the bike is currently turning
        //the turn input is + when turning right, - when turning left
        balanceShiftRate += inputDir.x * (turnSpeed * turningBalanceModifier);

        //This affects how quickly you regain balance:

        //Balance naturally attempts to return to 0 if the player is not turning significantly
        if (Mathf.Abs(inputDir.x) <= 0.25f)
        {
            if (currBalance < 0) //If we're on the left side of balance between, move towards the right -> 0
                balanceShiftRate += 1;
            else if (currBalance > 0) //and vice versa
                balanceShiftRate -= 1;
        }

        UpdateBalance(balanceShiftRate);
    }

    //Update the bike's current balance value
    //Sets any variables, and call any methods neccessary for that update
    public void UpdateBalance(float changeInBalance)
    {
        //Do nothing if we're already fallen
        if (isFallen) 
            return;

        //Clamp the balance value between constant limits
        currBalance = Mathf.Clamp(currBalance + changeInBalance, -MAX_BALANCE_LIMIT, MAX_BALANCE_LIMIT);

        //Rotate the bike model based on the current balance
        //Temporary "animation" for visual aid
        RotateBikeModel((currBalance / MAX_BALANCE_LIMIT) * MAX_BALANCE_ROTATION);

        // If the bike has entered the "balance danger zone," then flag it and start the related timer
        if (!isInBalanceDangerZone && Mathf.Abs(currBalance) >= balanceDangerThreshold)
        {
            isInBalanceDangerZone = true;
            balanceDangerTimer.StartTimerForSeconds(dangerZoneTimeLimit);
        }
    }

    //Checks if the player is currently within the balance danger zones, handles related timers.
    //This way, if you cross the danger zone, you dont just immediately fall over
    //Instead, you're on a timer to correct yourself, if you remain in the danger zone, youll fall over
    public void BalanceDangerCheck()
    {
        //If player is not in the "balance danger zone" then do nothing
        if (!isInBalanceDangerZone || isFallen)
            return;

        //If the player has remained in the "balance danger zone" for too long then they fall over
        if (balanceDangerTimer.checkTime())
        {
            Debug.Log("You fell off balance!");
            FallOver();
            isInBalanceDangerZone = false;
            balanceDangerTimer.CancelTimer();
            return;
        }

        //If the player is at risk of losing their balance BUT is no longer in the "balance danger zone,"
        //then their timer which records how long they have been in the "balance danger zone" should begin
        Debug.Log("TIME LEFT UNTIL BALANCE LOST: " + balanceDangerTimer.timeLeft);
        if (Mathf.Abs(currBalance) < balanceDangerThreshold)
        {
            //If the player has stayed out of the "balance danger zone" until the timer ran out, then they are no longer considered in danger
            if (balanceDangerTimer.timeLeft > dangerZoneTimeLimit)
            {
                Debug.Log("Balance restored!");
                isInBalanceDangerZone = false;
                balanceDangerTimer.CancelTimer();
            }
            //Else, add seconds back to the timer
            else
                balanceDangerTimer.AddSecondsToTimer(Time.deltaTime * 2);
        }
    }

    //Rotates just the model attached to the player by a rotation in degrees
    //attempts to shift it down to match rotation so it appears to stay on ground
    public void RotateBikeModel(float xRotation)
    {
        Quaternion modelRotation = Quaternion.Euler(new Vector3(xRotation, 90, 0));
        bikeModel.localRotation = modelRotation; //rotate it

        float xShiftAmount = (xRotation / FALLEN_OVER_ROTATION) * 1.9f;
        float yShiftAmount = (Mathf.Abs(xRotation) / FALLEN_OVER_ROTATION) * 0.8f;
        if (Mathf.Abs(xRotation) >= 75)
            yShiftAmount = 1.4f;
        bikeModel.localPosition = new Vector3(xShiftAmount, -yShiftAmount, 0); //shift it down
    }

    //When a player falls over, they have to drop the ball if they were holding, turn off pickup zones to prevent pickup,
    //and rotate the bike to a fallen over status (temporary "animation")
    //Alter its max speed / or prevent movement?
    public void FallOver()
    {
        //Option: Set speed to 0 to prevent movement while fallen
        speed = 0;

        mallet.DropBall();
        mallet.enableZones(false);

        isFallen = true;

        if (currBalance < 0)
            RotateBikeModel(-FALLEN_OVER_ROTATION);
        else
            RotateBikeModel(FALLEN_OVER_ROTATION);
    }

    //When a player gets up from a fallen status, need to reset any bools
    //Reset the current balance back to defualt, turn theyre pick up zones back on
    public void GetUp()
    {
        RotateBikeModel(0);
        currBalance = 0;
        isFallen = false;
        mallet.enableZones(true);
    }

    //Dash that has a cooldown, locks you into direction, and lasts for a determined number of seconds
    //Cant dash while holding ball, cant dash while going too slow
    public void Dash()
    {
        isDashing = true;

        //start cooldown timer
        dashDurationTimer.StartTimerForSeconds(dashDuration);

        //alter our speed
        speed = dashingSpeedModifier * maxSpeed;
        turnSpeed = dashTurnSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        //Dashing into range of ball makes Player auto pick up the ball without inputs
        if (isDashing && mallet.ballInZone)
        {
            mallet.HoldBall(); 
        }

        //Cooldown handling
        if (isDashing && dashDurationTimer.checkTime())
        {
            isDashing = false;
            dashCooldownTimer.StartTimerForSeconds(dashCooldown);
        }

        inputDir = GetInputDirection();

        if (!isDashing && !isFallen)  //Speed and turn speed are constants when dashing, no need to calculate
        {
            CalculateSpeed(inputDir.z);
            CalculateTurnSpeed();
        }

        CalculateBalanceShift();
        BalanceDangerCheck();
        GetButtonInputs();
    }
    void FixedUpdate()
    {
        MovePositioRB(inputDir); //Move based on input direction

        if (mallet.holdingBall)
        {
            lastFollowTargetPos = mallet.aimDirection * mallet.currentLineLength;
        }
        if (mallet.currentZone != null)
        {
            followTarget.position = mallet.currentZone.holdSpot + lastFollowTargetPos;
        }

        updateHoldingState();
    }

}
