using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HingeScript : MonoBehaviour {
    private Transform blade;
	// Use this for initialization
	void Start () {
 
        blade = transform.parent.Find("Blade").transform;
	}
	
	// Update is called once per frame
	void Update () {
        Vector2 direction = new Vector2(blade.localPosition.x, blade.localPosition.z);
        //print(direction);
        direction.Normalize();
        //print(direction);
        transform.localRotation = Quaternion.FromToRotation(new Vector3(0,0,1), new Vector3(direction.x, 0, direction.y));
        //transform.parent.GetComponent<Rigidbody>().ApplyEffectiveTorqueAtRelativePosition(transform.parent.TransformDirection(Vector3.left*-0.5f), new Vector3(0,0.52f,0));
        //Debug.DrawRay(transform.parent.TransformPoint(new Vector3(0, 0.52f, 0)), transform.parent.TransformDirection(Vector3.left), Color.cyan);
    }
}
