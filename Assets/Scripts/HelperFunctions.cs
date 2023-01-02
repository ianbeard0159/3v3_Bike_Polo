using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperFunctions 
{


    //If given a value inside the range, will return the closer value of values min or max
    //If given a value outside the defined range, will return the value 
    //Used to check if something is not within a range
    public static float reverseClamp(float value, float min, float max)
    {
        if (value <= min || value >= max)
            return value;

        float middleVal = (max + min)/2;

        if (value <= middleVal)
            return min;
        else 
            return max;
    }
}
