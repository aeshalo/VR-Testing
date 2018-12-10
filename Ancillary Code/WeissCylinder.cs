using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeissCylinder : MonoBehaviour {
    public float Target;
    public float speed;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Target >= 360)
        {
            Target -= 360;
        }
        else if (Target < 0)
        {
            Target += 360;
        }
        float compDiff = RotDir(transform.localEulerAngles.z, Target);
        //print("Current Position: " + transform.localEulerAngles.z + " Target: " + Target + " CompDiff: " + compDiff);
        if (Mathf.Abs(transform.localEulerAngles.z - Target) < 1)
        {
            //print("difference less than 5");
        }
        else if (compDiff > 0)
        {
            if (speed < Mathf.Abs(compDiff))
            { 
                transform.Rotate(0, 0, speed);
                //print("Rotate Positive");
            }
            else
            {
                transform.localEulerAngles = new Vector3(0, 0, Target);
                //print("Got to TGT");
            }
        }
        else if(compDiff < 0)
        {
            if (speed < Mathf.Abs(compDiff))
            {
                transform.Rotate(0, 0, - speed);
                //print("Rotate Negative");
            }
            else
            {
                transform.localEulerAngles = new Vector3(0, 0, Target);
                //print("Got to TGT");
            }
        }
	}

    float RotDir(float current,float target)
    {
        float compensator = 180 - target;

        float compcurrent = current + compensator;

        if (compcurrent >= 360)
        {
            compcurrent -= 360;
        }
        else if (compcurrent < 0)
        {
            compcurrent += 360;
        }
        return (-compcurrent + 180);
    }
    
}
