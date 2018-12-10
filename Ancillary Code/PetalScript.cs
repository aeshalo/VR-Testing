using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetalScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine( Die(15) );
	}
	
	// Update is called once per frame


    private IEnumerator Die(float waitTime)
    {
        yield return new WaitForSecondsRealtime(waitTime);
        Destroy(gameObject);
    }
}
