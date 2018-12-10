using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlyphScript : MonoBehaviour {
    private float scale;
    public bool destroyed;
    public float ScaleMultiplier;
    public float speed;
	// Use this for initialization
	void Start () {
        destroyed = false;
        scale = (float)0.01;
        transform.localScale = new Vector3((float)0.01, 1, (float)0.01);
	}
	
	// Update is called once per frame
	void Update () {
        
        if (transform.localScale.x < 0.4 * ScaleMultiplier && destroyed == false)
        {
            if (transform.localScale.x < 0.35 * ScaleMultiplier)
            {
                scale += (float)0.01 * ScaleMultiplier;
                transform.localScale = new Vector3(scale, 1, scale);
            }
            else if (transform.localScale.x >= 0.35 * ScaleMultiplier)
            {
                //print("LocalScale: " + transform.localScale.x + " dropping by: " + Mathf.Max((float)0.01 * (((float)0.4 - transform.localScale.x) * (float)20), (float)0.001));
                scale += Mathf.Max((float)0.01 * (((float)0.4 - transform.localScale.x)*(float)20) * ScaleMultiplier, (float)0.001);
                
                transform.localScale = new Vector3(scale, 1, scale);
            }
            transform.Rotate(0, speed*(1f +2.5f*(0.4f- transform.localScale.x / ScaleMultiplier)), 0);
        }
        else if (destroyed && gameObject.GetComponent<Renderer>().material.color.a > 0)
        {
            transform.Rotate(0, speed, 0);
            //print("Alpha: " + (gameObject.GetComponent<Renderer>().material.color.a - (float)0.5));
            transform.localScale = new Vector3(transform.localScale.x + 0.002f * ScaleMultiplier, 1, transform.localScale.x + 0.002f * ScaleMultiplier);
            gameObject.GetComponent<Renderer>().material.color = new Color(
                gameObject.GetComponent<Renderer>().material.color.r,
                gameObject.GetComponent<Renderer>().material.color.g,
                gameObject.GetComponent<Renderer>().material.color.b,
                Mathf.Max(gameObject.GetComponent<Renderer>().material.color.a - (float)0.05,0));
                //0);

        }
        else if(destroyed)
        {
            transform.Rotate(0, speed, 0);
            Destroy(gameObject);
        }
        else
        {
            transform.Rotate(0, speed, 0);
            
        }
        
	}
}
