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

    //Changes the speed limits of the bike
    [SerializeField] private float maxSpeed = 20;
    [SerializeField] private float minSpeed = 0;

    //Changes how quickly bike speeds up, slows down
    [SerializeField] private float acceleration = .06f; //How quickly to reach max speed when speeding up
    [SerializeField] private float deceleration = .04f; //How quickly yo reach reache min speed when slowing down

    //Changes how quickly bike turns
    [SerializeField] private float turnSpeedModifer = .3f; //Current speed affects how quickly you turn
    [SerializeField] private float turnSpeed = 160; //How quickly the bike turns when pressing left/right                                             

    //Changes how much force is applied to the bike when there was a simple collision with bike
    [SerializeField] private float pushForce = 40f; //How much force is applied to the ball if its pushed by the Bike

    //The current speed/velocity the bike is going
    private float speed = 0;
    public Vector3 currVel //Only used to give to Mallet so shots stay accurate
    {
        get { return transform.forward * speed; }
    }

    //Booleans used to animation, logic, etc
    public bool isDashing;

    //unused for now
    public float currentBalance = 100; //Based on some equation of speed, maybe turning status, and maybe button presses??

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
                //Debug.Log(key + " is valid, setting animation bool true");
                animationController.SetBool(key, true);
            }
            else {
                //Debug.Log(key + " is invalid, setting animation bool false");
                animationController.SetBool(key, false);
            }
        }
    }

    private Vector3 GetInputDirection()
    {
        float turnAmount = 0;
        float driveAmount = 0;

        if (!isDashing)
        {
            turnAmount = Input.GetAxis("Turn"); //Left/Right keys, A/D keys, or Left joystick on controller
            driveAmount = Input.GetAxis("Drive"); //Down/Up, S/W or Left/Right trigger on controller
        }

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
        //Mathf.Clamp(currentBalance, 0, 100);

        //if(speed >= maxSpeed / 2)
        //    {
        //        currentBalance--;
        //    }

        //if(speed < maxSpeed / 2)
        //    {
        //        currentBalance++;
        //    }
    }

    //Calculates and sets what the current speed is
    public void CalculateSpeed(float zInput)
    {
        if (isDashing)
        {
            //dont even both with calculating speed, we have a set speed while dashing
            return;
        }

        if(speed > maxSpeed)
        {
            speed = maxSpeed;
        }

        //Press up or down?
        if (zInput > 0 && speed < maxSpeed) //Speeding up with up key up right trigger
        {
            if (speed < maxSpeed) //Speed is less than max speed, speed up
            {
                speed += acceleration;
            }
            else
            {
                speed = maxSpeed; //No speeding up, already Too fast
            }
        }
        if (zInput < 0) //Slowing down with down key or right trigger, maybe add reverse here
        {
            if (speed > minSpeed) //Speed is more than the min speed, slow down
            {
                speed -= deceleration;
            }
            else
            {
                speed = minSpeed; //No slowing down, already Too slow
            }
        }
        if (zInput == 0) //Not pressing any keys, slow down more gradually to a halt
        {
            if (speed > 0)
            {
                speed -= deceleration / 2;
            }
            else
            {
                speed = 0;
            }
        }

        mallet.currVel = currVel;
        turnSpeed = (speed * speed) * turnSpeedModifer + 20;

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
            lastFollowTargetPos = mallet.aimDirection * mallet.aimLineLength;
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
        CalculateSpeed(inputDir.z);
        CalculateBalance();
        GetButtonInputs();
    }
}
