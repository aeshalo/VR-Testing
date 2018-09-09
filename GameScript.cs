using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScript : MonoBehaviour {
    private GameObject sword;
    private GameObject dummy;
    //private GameObject dummy2;
    // Use this for initialization
    void Start () {
        sword = GameObject.Find("WeissRapier").gameObject;
        dummy = GameObject.Find("Practice Drone").gameObject;
        //dummy2 = GameObject.Find("Practice Drone (1)").gameObject;
        print(sword + " " + dummy);
    }
	
	// Update is called once per frame
	void Update () {
		if (sword != null && sword.GetComponent<WeissSword>().isPickedUp)
        {
            //dummy.GetComponent<Rigidbody>().freezeRotation = false;
            dummy.GetComponent<HoverDummyScript>().enabled = true;
            //dummy2.GetComponent<HoverDummyScript>().enabled = true;
            sword = null;
            print("Starting to swing!");
            transform.GetComponent<GameScript>().enabled = false;
        }
	}
}
