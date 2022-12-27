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
    public Vector3 currentVel; 

    public float currentBalance; //Based on some equation of speed, maybe turning status, and maybe button presses??

    public bool useRB = true; //for testing purposes
    public bool useTranslate = false; //for testing purposes, changes the movement system to using transfrom.Translate()

    private Animator animationController;
    private List<string> holdingStates = new List<string>() {
        "BallHeldLeft",
        "BallHeldRight"
    };
    private MalletZone currentZone;

    // Start is called before the first frame update
    void Start()
    {
        t = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        mallet = gameObject.transform.Find("Mallet").gameObject.GetComponent<MalletController>();
        animationController = GetComponent<Animator>();
        setHoldingState("Normal");
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

    private Vector3 getInputDirection()
    {
        float x = Input.GetAxis("Horizontal"); //UpKey, W, or Forward on left joystick
        float z = Input.GetAxis("Vertical"); //DownKey, S, or Back on left joystick
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

    //Calculates and sets what the current speed is
    public void calculateSpeed(float zInput)
    {
        //Press up or down?
        if (zInput > 0 && speed < maxSpeed) //Speeding up with up key 
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
        if (zInput < 0) //Slowing down with down key, maybe add reverse here
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

    public void TranslateMove(Vector3 direction)
    {
        //calculateSpeed(inputDir.z);
        //turnSpeed = (speed * speed) * turnSpeedModifer + 20;
        t.Rotate(new Vector3(0, direction.x * Time.deltaTime * turnSpeed, 0));

        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
    public void MovePositioRB(Vector3 direction)
    {
        Quaternion turn = Quaternion.Euler(0, inputDir.x * Time.fixedDeltaTime * turnSpeed, 0);

        rb.MoveRotation(rb.rotation * turn);
        rb.MovePosition(rb.position + (transform.forward * speed * Time.fixedDeltaTime));
    }
    private void updateHoldingState() {
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

    void FixedUpdate()
    {
        if (currentZone != mallet.currentZone) {
            updateHoldingState();
        }
        if (rb == null)
        {
            return;
        }
        if (useRB)
        {
            MovePositioRB(inputDir); //Move based on input direction
        }
    }

    // Update is called once per frame
    void Update()
    {
        inputDir = getInputDirection();

        calculateSpeed(inputDir.z);
        //currentBalance = 4.5f * speed - 90;

        if (useTranslate)
        {
            useRB = false;
        }
        if (useRB)
        {
            useTranslate = false;
        }
           
        if (useTranslate)
        {
            TranslateMove(inputDir);
        }
    }
}
