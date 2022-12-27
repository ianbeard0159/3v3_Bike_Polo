using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeController : MonoBehaviour
{
    private Transform t;
    private Rigidbody rb;
    private Vector3 inputDir;

    //All these values can be tweaked to change the feel of the movement
    [SerializeField] public float speed = 0;
    [SerializeField] private float maxSpeed = 20;
    [SerializeField] private float minSpeed = 0;
    [SerializeField] private float acceleration = .06f; //How quickly spped max speed when speeding up
    [SerializeField] private float deceleration = .04f; //How quickly speed reaches 0 when slowing down

    [SerializeField] private float turnSpeedModifer = .3f; //Curren speed affects how quickly you turn
    [SerializeField] private float turnSpeed = 160; //How quickly the bike turns when pressing left/right
                                                    //Balance will have to do with current turn speed as well

    [SerializeField] private float pushForce = 40f; //How much force is applied to the ball if its pushed by the Bike

    //Used just to set current velocity of Mallet:
    MalletController mallet;
    private Vector3 currentVel;

    public float currentBalance = 100; //Based on some equation of speed, maybe turning status, and maybe button presses??

    private Animator animationController;
    private List<string> holdingStates = new List<string>() {
        "BallHeldLeft",
        "BallHeldRight"
    };
    private MalletZone currentZone;
    private Transform followTarget;

    // Start is called before the first frame update
    void Start()
    {
        t = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        
        mallet = gameObject.transform.Find("Mallet").gameObject.GetComponent<MalletController>();
        animationController = GetComponent<Animator>();
        setHoldingState("Normal");
        followTarget = gameObject.transform.Find("Follow Target").transform;
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

        Debug.Log("UpKey/W or Right Trigger on controller to Move forward/Accelerate");
        Debug.Log("DownKey/S or Left Trigger on controller to Slow down/Break");
        Debug.Log("Left/Right Key, A/D or Tilt Left Stick Left/Right on controller to Turn");
    }

    private Vector3 GetInputDirection()
    {
        float x = Input.GetAxis("Turn"); //Left/Right keys, A/D keys, or Left joystick on controller
        float z = Input.GetAxis("Drive"); //Down/Up, S/W or Left/Right trigger on controller
        Vector3 direction = new Vector3(x, 0, z);
        return direction;
    }

    //Pushes the ball with a force defined by pushForce if bike collides with it
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            Vector3 direction = (collision.gameObject.transform.position - transform.position);
            collision.rigidbody.AddForce(pushForce * direction);
        }
    }

    public void CalculateBalance()
    {
        Mathf.Clamp(currentBalance, 0, 100);

        if(speed >= maxSpeed / 2)
        {
            currentBalance--;
        }

        if(speed < maxSpeed / 2)
        {
            currentBalance++;
        }
    }

    //Calculates and sets what the current speed is
    public void CalculateSpeed(float zInput)
    {
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

        currentVel = transform.forward * speed;
        mallet.currVel = currentVel;
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
        if (!mallet.holdingBall) {
            setHoldingState("Normal");
            return;
        }
        currentZone = mallet.currentZone;
        switch (currentZone?.name) {
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

    public void Dash()
    {

    }

    public void GetButtonInputs()
    {

    }

    void FixedUpdate()
    {
        MovePositioRB(inputDir); //Move based on input direction

        if (mallet.holdingBall) {
            followTarget.position = mallet.currentZone.holdSpot + mallet.aimDirection * mallet.aimLineLength;
        }
        else {
            followTarget.position = gameObject.transform.position;
        }

        updateHoldingState();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.DrawRay(transform.position, transform.forward * 30, Color.blue, 1);
        inputDir = GetInputDirection();
        CalculateSpeed(inputDir.z);
        CalculateBalance();
        GetButtonInputs();
    }
}
