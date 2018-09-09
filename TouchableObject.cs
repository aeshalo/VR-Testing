using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchableObject : MonoBehaviour {
    public bool isPickedUp;
    public GameObject holder;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void pickup(GameObject hand)
    {
        isPickedUp = true;
        holder = hand;
    }

    public void letgo()
    {
        isPickedUp = false;
    }
}
