using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingDummyScript : MonoBehaviour {
    public Vector3 PIDNormal;
    public Vector3 PIDTangental;
    public GameObject vectorprefab;
    private GameObject blade;
    private GameObject Head;
    private Vector3 hinge = new Vector3(0, 1.32f, 0);
    public bool swinging = false;
    public bool swinglock = false;
    public bool backlock = false;
    private Vector3 swingPlaneVec;
    public float pidI1;
    public float pidI2;
    //private GameObject[] vectors;
    private Vector3 prevVelocity;
    // Use this for initialization
    void Start()
    {
        //vectors = new GameObject[10];
        blade = gameObject;
        Head = GameObject.Find("[CameraRig]").transform.Find("Camera (eye)").gameObject;
        hinge = transform.parent.transform.position + hinge;
        //print("hinge(world): " + hinge);
    }
	// Update is called once per frame
	void Update ()
    {
        Vector3 TgtVec = Head.transform.position - hinge - new Vector3(0,Random.Range(0, 0.4f),0);
        Vector3 BldVec = blade.transform.position - hinge;
        //print("Vectors. TgtVec: " + TgtVec + " BldVec: " + BldVec);
        if (swinging == false && Head != null)
        {
            float angle = Random.Range(-60, 60);
            //print("Angle Chosen: " + angle);
            Vector3 x = Quaternion.AngleAxis(angle, TgtVec) * Vector3.ProjectOnPlane(BldVec,TgtVec);
            //print("Rotated Vector: " + x);
                swingPlaneVec = Vector3.Normalize(Vector3.Cross(TgtVec, x));
            swinging = true;
            //print("Swing Plane normal Vector: " + swingPlaneVec);
        }
        //print(Vector3.Dot(TgtVec, BldVec));

        if (swinging && (Vector3.Dot(TgtVec, BldVec) < -1.1) && swinglock == false) 
        {

            swinging = false;
            swinglock = true;
        }
        else if (swinging && (Vector3.Dot(TgtVec, BldVec) > -1.1) && swinglock == true)
        {
            swinglock = false;
        }
        
    }
    void FixedUpdate()
    {
        if (swinging) {
            Vector3 bladevec = blade.transform.position - hinge;
            float planeOffset = Vector3.Dot(bladevec, swingPlaneVec);
            pidI1 += planeOffset;
            float planeRvel = Vector3.Dot(blade.GetComponent<Rigidbody>().velocity, swingPlaneVec);
            //print(blade.GetComponent<Rigidbody>().velocity);
            Vector3 PlnNorVelVec = Vector3.Cross(swingPlaneVec,bladevec);

            Vector3 ControlSignalN =swingPlaneVec * (planeOffset * PIDNormal[0] + pidI1 * PIDNormal[1] + planeRvel * PIDNormal[2]) + PlnNorVelVec*-PIDTangental[0];

            
            blade.GetComponent<Rigidbody>().AddForce(ControlSignalN*-3f);
            blade.GetComponent<Rigidbody>().AddRelativeTorque(0, Vector3.Dot(swingPlaneVec, blade.transform.forward)*0.15f, 0);
        }
        if (swinging && (Vector3.Dot(Vector3.Normalize(Head.transform.position - hinge - new Vector3(0, 0.25f, 0)), blade.transform.position - hinge) > 0) && Vector3.Dot(prevVelocity,blade.GetComponent<Rigidbody>().velocity) < 0.7 && backlock == false)
        {
            swinging = false;
            backlock = true;
            //print("stopped");
        }
        else if ((swinging && blade.GetComponent<Rigidbody>().velocity.sqrMagnitude > 1) && backlock == true)
        {
            backlock = false;
        }
        prevVelocity = blade.GetComponent<Rigidbody>().velocity;
    }

    
}



/* MODUS OPERANDI
 *   pick a direction rand(+45 -45) constarined to local 170/-170
 *   apply force and swing  in that direction towards PLAYER'S HEAD
 *   calculate  a plane normal to swing axis and tanget to line hinge --> head
 * wait until:
 *  A: blade is stopped WRT plane (blocked!)
 *      then: go backwards and pick a new direction
 *  B: blade goes through plane
 *      then:  continue swing + pick another direction?
 * 
 * INFO NEEDED: blade trn, head pos, hinge pos.
 *
 */

