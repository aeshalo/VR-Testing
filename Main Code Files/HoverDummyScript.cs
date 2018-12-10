using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HoverDummyScript : Damagable {

    //GAMEPLAY
    private bool Frozen;






    //PHYSICS
    //max actuator forces for PIDs
    public Vector4 HOMBMaxForce;
    //a PID
    public Vector3 HoverPID;
    public float HoverAntiWindup;
    private float HoverI;
    //a PID
    public Vector3 OrientPID;
    private float OrientI;
    //a PID
    public Vector3 MovePID;
    private float ForwardI;
    //anotherPID
    public Vector3 BladePID;
    private float BladeI;
    //some settable constants
    public float HoverHeight;
    public float restDistance;
    public float strikeDistance;
    public float BladePower;
    private bool striking;
    //and some global variables //references
        //blade
    private bool swinging;
    private Vector3 swingPlaneVec;
    private GameObject blade;
    private float angle;
    private bool swung;
    private float NeckOffset;
    private Vector3 MemHeadPos;
        //body
    public int leftRight;
    private Rigidbody body;
    private GameObject head;
    
    private Vector3 hinge;
    private Vector3 restDir;
    private float sqrDistance;
    // Use this for initialization
    void Start () {
        //body control
        body = transform.GetComponent<Rigidbody>();
        head = GameObject.Find("[CameraRig]").transform.Find("Camera (eye)").gameObject;
        // blade control
        blade = transform.Find("Blade").gameObject;
        hinge = new Vector3(0, 0.52f, 0);
        striking = false;
        angle = 0;
        NeckOffset = 0;
    }
	
	// Update is called once per frame
	void Update ()
    {
        //Behaivior Logic
        //dummy is either strafing or striking
        
        //if the dummy can't reach the player, it shouldn't be striking.
        if (sqrDistance > 1.2*Mathf.Pow(restDistance,2) && !swinging)
        {

            striking = false;
        }
        else if (!striking)
        {
            //otherwise it will strike at random. watch out!
            if (Random.Range(1L, 100L) > 99f)
            {
                //print("Striking!");
                striking = true;
                //strike a semi-random point in the player's torso area, and swing from either the left or right.
                NeckOffset = Random.Range(0.1f, 0.6f);
                leftRight = Random.Range(-8, 10) - 1;
            }
        }
        //this allows the player to apply status effects to the dummy. more than one at a time, too. [WIP]
        string[] StateWords = State.Split(' ');
        if (StateWords[0] == "Frozen")
        {
            //if frozen, freeze!
            Frozen = true;
            StartCoroutine(ExtentionMethods.Cooldown(float.Parse(StateWords[1]), (x) => Frozen = !x)); //fancy wait statement. reusable!
            body.drag = 10000000;
            blade.GetComponent<Rigidbody>().drag = 10000000;
            State = "";
        }
        if(!Frozen && body.drag == 10000000)
        {
            //Unfreeze when coroutine triggers
            body.drag = 0;
            blade.GetComponent<Rigidbody>().drag = 0;
        }
        
    }

    void FixedUpdate() //When dealing with Physics, use Fixedupdate. 
    {
        //HOVER
        if (!swinging)
        {
            //where is the player's head? if i'm in the middle of swing, don't update.
            MemHeadPos = head.transform.position;
        }
        //this is the difference from the setpoint. used in the PID
        float offset = (HoverHeight - transform.position.y);
        //print(offset);
        //Hover PID implementation
        HoverI += offset;
        Mathf.Clamp(HoverI, -HoverAntiWindup, HoverAntiWindup); //Anti windup
        float deriv = -body.velocity.y;
        float HoverSignal = (offset * HoverPID[0] + HoverI * HoverPID[1] + deriv * HoverPID[2]); //actuator signal
        body.AddForce(transform.up * Mathf.Min(HoverSignal, HOMBMaxForce[0])); //artificial actuator saturation

        // Orient

        //figure out where the playe is from the dummy, what direction to face etc...
        Vector3 HorizCross = Vector3.Cross(Vector3.Normalize(new Vector3(MemHeadPos.x - body.position.x, 0, MemHeadPos.z - body.position.z)), Vector3.Normalize(new Vector3(transform.forward.x, 0, transform.forward.z)));
        //Debug.DrawRay()
        Vector3 OriOffset = Vector3.Cross(transform.up, Vector3.up) + 0.2f * HorizCross;
        //Debug.DrawRay(transform.position + new Vector3(0, 0.68f, 0), OriOffset,Color.blue);
        //Debug.DrawRay(transform.position + new Vector3(0, 0.68f, 0), HorizCross,Color.red);

        //another PID 
        Vector3 OriDeriv = -body.angularVelocity;
        OrientI += Vector3.Dot(transform.up, Vector3.up);
        body.AddTorque(Vector3.ClampMagnitude(OriOffset * (OrientPID[0] + OrientI * OrientPID[1]) + OriDeriv * OrientPID[2], HOMBMaxForce[1]));
        //print(Mathf.Min(HoverSignal, HOMMaxForce[0]));
        //print(Mathf.Min(Mathf.Min(OrientPID[0] + OrientI * OrientPID[1], HOMMaxForce[1])));
        
        //Move


        //forward
        //how far away is the player? don't sqrroot to save time
        sqrDistance = new Vector2((MemHeadPos.x - transform.position.x), (MemHeadPos.z - transform.position.z)).sqrMagnitude;
        float setPoint;

        //move in to strike
        if (striking) { setPoint = Mathf.Pow(strikeDistance, 2);}
        //move out when strafing
        else { setPoint = Mathf.Pow(restDistance, 2); }
        //PID setpoint
        float distance = sqrDistance - setPoint;

        //PID
        ForwardI += distance;
        body.AddForce(Vector3.ClampMagnitude(Vector3.Normalize(new Vector3((MemHeadPos.x - transform.position.x), 0, (MemHeadPos.z - transform.position.z))) * (distance * MovePID[0] + ForwardI * MovePID[1]), HOMBMaxForce[2]) + new Vector3(body.GetComponent<Rigidbody>().velocity.x, 0, body.GetComponent<Rigidbody>().velocity.z) * -MovePID[2]);
        
        //strafe around the player
        if (!striking)
        {
            //print("rotating");
            body.AddForce(transform.right * (6/sqrDistance) * leftRight );
        }
        //BLADE

        //vector dummy --> player head
        Vector3 TgtVec = MemHeadPos - (transform.TransformPoint(hinge)) - new Vector3(0, NeckOffset, 0);
        
        //vector along blade
        Vector3 BldVec = (blade.transform.position - (transform.TransformPoint(hinge))); //CHECKED
        
        //when not in use, blade should rest behind dummy.
        restDir = transform.forward; 
        
        //strike has been activated. set up.
        if (striking && !swinging)
        {
            angle = Random.Range(-120, 120); //random cutting angle
            //print("Angle Chosen: " + angle);
            BladeI = 0; //RESET
            swinging = true; //blade is now swinging towards player
            //print("Swing Plane normal Vector: " + swingPlaneVec);
        }

        //strike is occuring. swing the blade!
        else if (swinging)
        {
            //vector normal to swinging plane
            Vector3 x = Quaternion.AngleAxis(angle, TgtVec) * Vector3.ProjectOnPlane(Vector3.up, TgtVec);
            
            //print("Rotated Vector: " + x);
            swingPlaneVec = Vector3.Normalize(Vector3.Cross(TgtVec, x));
            if(!striking) { swingPlaneVec = swingPlaneVec * -1f; }
            //Debug.DrawRay(transform.TransformPoint(hinge), swingPlaneVec, Color.green);

            //keep the balde in-plane and swing forward through plane
            float CosOffset = Vector3.Dot(BldVec,swingPlaneVec);
        
            BladeI += CosOffset;
            Vector3 EffectiveTorque = Vector3.ClampMagnitude(Vector3.Cross(Vector3.Normalize(BldVec),swingPlaneVec) * (BladePID[0] * CosOffset + BladePID[1] * BladeI + Vector3.Dot(blade.GetComponent<Rigidbody>().velocity, swingPlaneVec) * BladePID[2]) +  swingPlaneVec *BladePower, HOMBMaxForce[3]);
            blade.GetComponent<Rigidbody>().ApplyEffectiveTorqueAtRelativePosition(EffectiveTorque,new Vector3(0f,-0.68f,0f));
            body.ApplyEffectiveTorqueAtRelativePosition(-1f * EffectiveTorque, new Vector3(0, 0.52f, 0));
            blade.GetComponent<Rigidbody>().AddRelativeTorque(0, Vector3.Dot(swingPlaneVec, blade.transform.forward) * 0.10f, 0);
            //Debug.DrawRay(transform.TransformPoint(hinge), EffectiveTorque, Color.white);

            //print(Vector3.Dot(BldVec, restDir));

            //if the balde's velcoity is significantly disrupted, it's hit something. swing back when that happens
            if (!swung && Vector3.Dot(BldVec, restDir) < 0) {
                swung = true;
                //print("swung");
            }
            else if(swung && Vector3.Dot(BldVec, restDir) > 0.1)
            {
                //print("swung back");
                swung = false;
                striking = false;
                swinging = false;
                BladeI = 0;
            }
            if (swung)
            {
                if ((blade.GetComponent<Rigidbody>().velocity - body.velocity).sqrMagnitude < 0.25 || Vector3.Dot(Vector3.Cross(blade.GetComponent<Rigidbody>().velocity - body.velocity, BldVec), swingPlaneVec) < 0)
                {
                    //print("deflected!");
                    striking = false;
                }
            }
            
        }
        
        else if (!swinging && !striking && !swung)
        {
            float offsetAngle = Vector3.Angle(restDir, BldVec);
            //Debug.DrawRay(transform.TransformPoint(hinge), restDir, Color.white);
            Vector3 RotAxis = Vector3.Normalize(Vector3.Cross(restDir, BldVec));
            Vector3 EffectiveTorque = Vector3.ClampMagnitude(RotAxis * (BladePID[0] * offsetAngle * 0.1f  + 0.01f * BladePID[1] * BladeI) +  Vector3.Cross((blade.GetComponent<Rigidbody>().velocity - body.velocity),BldVec)  * -BladePID[2] , HOMBMaxForce[3]);
            //print(offsetAngle);
            blade.GetComponent<Rigidbody>().ApplyEffectiveTorqueAtRelativePosition(EffectiveTorque, new Vector3(0f, -0.68f, 0f));
            body.ApplyEffectiveTorqueAtRelativePosition(-1f * EffectiveTorque, new Vector3(0, 0.52f, 0));
            //blade.GetComponent<Rigidbody>().AddRelativeTorque(0, Vector3.Dot(swingPlaneVec, blade.transform.forward) * 0.10f, 0);
        }
        
    }
    //if the blade hit the players weapon, maybe do something (effects? change state machine?) [WIP]
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "PlayerWeapon")
        {
            //float CutDepth = collider.
            
        }
    }

}
