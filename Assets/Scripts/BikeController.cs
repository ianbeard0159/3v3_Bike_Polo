using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(MalletController))]
public class BikeController : MonoBehaviour
{
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

    //The current speed/velocity the bike is going
    private float speed = 0;
    public Vector3 currVel //Only used to give to Mallet so shots stay accurate
    {
        get { return transform.forward * speed; }
    }

    //Booleans used to animation, logic, etc
    public bool isDashing;

    //public float currentBalance = 100; //Based on some equation of speed, maybe turning status, and maybe button presses??

    //Neccessary components and others
    private Transform t;
    private Rigidbody rb;
    private MalletController mallet;
    private Animator animationController;
    private MalletZone currentZone;
    private Transform followTarget;
    private Vector3 lastFollowTargetPos = Vector3.zero;

    //Inputs
    private Vector3 inputDir;

    //Helper timers
    Timer dashDurationTimer;
    Timer dashCooldownTimer;

    private List<string> holdingStates = new List<string>() {
        "BallHeldLeft",
        "BallHeldRight"
    };

    private void Awake()
    {
        t = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = Vector3.zero;
        rb.inertiaTensorRotation = new Quaternion(0, 0, 0, 1);
        mallet = GetComponent<MalletController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        isDashing = false;

        dashDurationTimer = new Timer();
        dashCooldownTimer = new Timer();

        animationController = GetComponent<Animator>();
        setHoldingState("Normal");
        followTarget = gameObject.transform.Find("Follow Target").transform;

        //Debug.Log("Keyboard/Mouse: Dash on Shift, Hold/Shoot on Left Click, Switch Sides on Right Click");
        //Debug.Log("Keyboard/Mouse: WASD to Drive/Turn.");
        //Debug.Log("(Xbox) Controller: Triggers to Drive, Left Stick to Turn");
        //Debug.Log("(Xbox) Controller: Dash on X, Hold/Shoot on Left Bumper, Switch Sides on X (holding ball");
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

    //Determines direction of bike based on bike
    private Vector3 GetInputDirection()
    {
        float turnAmount = Input.GetAxis("Turn"); //Left/Right keys, A/D keys, or Left joystick on controller
        float driveAmount = Input.GetAxis("Drive"); //Down/Up, S/W or Left/Right trigger on controller

        Vector3 direction = new Vector3(turnAmount, 0, driveAmount);
        return direction;
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

    //TODO
    public void CalculateBalance()
    {

    }

   //Turn speed is based on speed
   //also decided by if Player is Dashing or not
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

    //Calculates and sets what the current speed is
    public void CalculateSpeed(float zInput)
    {
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

    public void MovePositioRB(Vector3 direction)
    {
        Quaternion turn = Quaternion.Euler(0, inputDir.x * Time.fixedDeltaTime * turnSpeed, 0);

        rb.MoveRotation(rb.rotation * turn);
        rb.MovePosition(rb.position + (transform.forward * speed * Time.fixedDeltaTime));
    }
    private void updateHoldingState() {
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

    //Dash that has a cooldown, locks you into direction, and lasts for a determined number of seconds
    //Cant dash while holding ball, cant dash while going too slow
    public void Dash()
    {
        dashDurationTimer.StartTimerForSeconds(dashDuration);
        isDashing = true;
        speed = dashingSpeedModifier * maxSpeed;
        turnSpeed = dashTurnSpeed;
    }

    public void GetButtonInputs()
    {
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

    void FixedUpdate()
    {
        MovePositioRB(inputDir); //Move based on input direction

        if (mallet.holdingBall) {
            lastFollowTargetPos = mallet.aimDirection * mallet.currentLineLength;
        }
        if (mallet.currentZone != null) {
            followTarget.position = mallet.currentZone.holdSpot + lastFollowTargetPos;
        }

        updateHoldingState();
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

        if (!isDashing)  //Speed and turn speed are constants when dashing, no need to calculate
        {
            CalculateSpeed(inputDir.z);
            CalculateTurnSpeed();
        }

        CalculateBalance();
        GetButtonInputs();
    }
}
