using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoMScript : MonoBehaviour {
    public Vector3 CoM;
    // Use this for initialization
    void Start () {
        GetComponent<Rigidbody>().centerOfMass = CoM;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
