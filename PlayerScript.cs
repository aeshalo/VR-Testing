using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour {
    public Quaternion rotation;
    private AudioSource[] clips;
    public GameObject petals;
	// Use this for initialization
	void Start () {
        clips = transform.GetComponents<AudioSource>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        rotation = transform.rotation;
        transform.rotation = Quaternion.identity;
	}


    private void OnTriggerEnter(Collider collider)
    {
        if( collider.tag == "weapon")
        {
            clips[(Random.Range(0, clips.Length - 1))].Play();
            Instantiate(petals, collider.GetComponent<Collider>().ClosestPoint(transform.position -new Vector3(0,0.5f,0)), Quaternion.FromToRotation(Vector3.back, collider.transform.GetComponent<Rigidbody>().velocity*-1));
        }
    }
}
