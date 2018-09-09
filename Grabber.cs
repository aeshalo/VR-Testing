using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabber : MonoBehaviour {
    public GameObject HeldObject;
    public GameObject ReachableObject;
    private SteamVR_TrackedObject hand;
    public SteamVR_Controller.Device Controller {   get { return SteamVR_Controller.Input((int)hand.index); }   }
    public bool slave;

    private bool doubleGrip;
    //public ParticleSystem system;
// Use this for initialization
    void Start () {
        hand = GetComponent<SteamVR_TrackedObject>();
        //system.Pause();
	}
	
	// Update is called once per frame
	void Update () {
        if (Controller.GetHairTriggerDown() && HeldObject == null) {
            //system.Play();
            //print("trigger is down");
            if (ReachableObject != null && ReachableObject.GetComponent<TouchableObject>().isPickedUp == false && !slave) {
                HeldObject = ReachableObject;
                HeldObject.GetComponent<TouchableObject>().pickup(gameObject);
                transform.Find("Model").gameObject.SetActive(false);
                transform.GetComponent<SphereCollider>().enabled = false;
            }
        }
        else
        {
            //system.Pause();
        }
        if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip) && !slave)
        {
            if(doubleGrip == true && HeldObject != null)
            {
                HeldObject.GetComponent<TouchableObject>().letgo();
                HeldObject = null;
                transform.Find("Model").gameObject.SetActive(true);
                transform.GetComponent<SphereCollider>().enabled = true;
            }
            else
            {
                doubleGrip = true;
                StartCoroutine(doubleClick((float)0.5));
            }
        }

    }
        private void OnTriggerEnter(Collider collision)
    {
        ReachableObject = collision.gameObject;
    }

        private void OnTriggerExit(Collider collision)
    {
        ReachableObject = null;
    }

    private IEnumerator doubleClick(float waitTime)
    {
        yield return new WaitForSecondsRealtime(waitTime);
        Debug.Log("Double Click" + waitTime);
        doubleGrip = false;
    }
}