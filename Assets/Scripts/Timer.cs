using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    public float amount;
    private float timeStamp = -1;

    //// Start is called before the first frame update
    //void Start()
    //{
    //    timeStamp = -1;
    //}

    //Not implemented
    public void startWatch()
    {
        timeStamp = Time.time;
    }

    //Not implemented
    public float StopWatch()
    {
        float time = 0;
        return time;
    }

    public void StartTimerForSeconds(float seconds)
    {
        timeStamp = Time.time + seconds;
        //Debug.Log("Starting a timer for " + seconds + " seconds");
    }
    public bool checkTime()
    {
        if (timeStamp > Time.time)
        {
            return false;
        }
        else
        {
            //Debug.Log("Timer is up!");
            return true;
        }
    }
}
