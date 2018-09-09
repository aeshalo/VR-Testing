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
    //dat state machine tho
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
        //dummy is either rotating or striking
 
        if (sqrDistance > 1.2*Mathf.Pow(restDistance,2) && !swinging)
        {

            striking = false;
        }
        else if (!striking)
        {
            
            if (Random.Range(1L, 100L) > 99f)
            {
                //print("Striking!");
                striking = true;
                NeckOffset = Random.Range(0.1f, 0.6f);
                leftRight = Random.Range(-8, 10) - 1;
            }
        }
        string[] StateWords = State.Split(' ');
        if (StateWords[0] == "Frozen")
        {
            Frozen = true;
            StartCoroutine(ExtentionMethods.Cooldown(float.Parse(StateWords[1]), (x) => Frozen = !x));
            body.drag = 10000000;
            blade.GetComponent<Rigidbody>().drag = 10000000;
            State = "";
        }
        if(!Frozen && body.drag == 10000000)
        {
            body.drag = 0;
            blade.GetComponent<Rigidbody>().drag = 0;
        }
        
    }

    void FixedUpdate()
    {
        //HOVER
        if (!swinging)
        {
            MemHeadPos = head.transform.position;
        }

        float offset = (HoverHeight - transform.position.y);
        //print(offset);
        HoverI += offset;
        Mathf.Clamp(HoverI, -HoverAntiWindup, HoverAntiWindup);
        float deriv = -body.velocity.y;
        float HoverSignal = (offset * HoverPID[0] + HoverI * HoverPID[1] + deriv * HoverPID[2]);
        body.AddForce(transform.up * Mathf.Min(HoverSignal, HOMBMaxForce[0]));

        // Orientate
        Vector3 HorizCross = Vector3.Cross(Vector3.Normalize(new Vector3(MemHeadPos.x - body.position.x, 0, MemHeadPos.z - body.position.z)), Vector3.Normalize(new Vector3(transform.forward.x, 0, transform.forward.z)));
        //Debug.DrawRay()
        Vector3 OriOffset = Vector3.Cross(transform.up, Vector3.up) + 0.2f * HorizCross;
        //Debug.DrawRay(transform.position + new Vector3(0, 0.68f, 0), OriOffset,Color.blue);
        //Debug.DrawRay(transform.position + new Vector3(0, 0.68f, 0), HorizCross,Color.red);
        Vector3 OriDeriv = -body.angularVelocity;
        OrientI += Vector3.Dot(transform.up, Vector3.up);
        body.AddTorque(Vector3.ClampMagnitude(OriOffset * (OrientPID[0] + OrientI * OrientPID[1]) + OriDeriv * OrientPID[2], HOMBMaxForce[1]));
        //print(Mathf.Min(HoverSignal, HOMMaxForce[0]));
        //print(Mathf.Min(Mathf.Min(OrientPID[0] + OrientI * OrientPID[1], HOMMaxForce[1])));
        //Move
        //forward
        sqrDistance = new Vector2((MemHeadPos.x - transform.position.x), (MemHeadPos.z - transform.position.z)).sqrMagnitude;
        float setPoint;

        if (striking) { setPoint = Mathf.Pow(strikeDistance, 2);}
        else { setPoint = Mathf.Pow(restDistance, 2); }
        float distance = sqrDistance - setPoint;
        ForwardI += distance;
        body.AddForce(Vector3.ClampMagnitude(Vector3.Normalize(new Vector3((MemHeadPos.x - transform.position.x), 0, (MemHeadPos.z - transform.position.z))) * (distance * MovePID[0] + ForwardI * MovePID[1]), HOMBMaxForce[2]) + new Vector3(body.GetComponent<Rigidbody>().velocity.x, 0, body.GetComponent<Rigidbody>().velocity.z) * -MovePID[2]);
        if (!striking)
        {
            //print("rotating");
            body.AddForce(transform.right * (6/sqrDistance) * leftRight );
        }
        //BLADE

        Vector3 TgtVec = MemHeadPos - (transform.TransformPoint(hinge)) - new Vector3(0, NeckOffset, 0);

        Vector3 BldVec = (blade.transform.position - (transform.TransformPoint(hinge))); //CHECKED

        restDir = transform.forward; 
        
        if (striking && !swinging)
        {
            angle = Random.Range(-120, 120);
            //print("Angle Chosen: " + angle);
            BladeI = 0;
            swinging = true;
            //print("Swing Plane normal Vector: " + swingPlaneVec);
        }
        else if (swinging)
        {
            Vector3 x = Quaternion.AngleAxis(angle, TgtVec) * Vector3.ProjectOnPlane(Vector3.up, TgtVec);
        
            //print("Rotated Vector: " + x);
            swingPlaneVec = Vector3.Normalize(Vector3.Cross(TgtVec, x));
            if(!striking) { swingPlaneVec = swingPlaneVec * -1f; }
            //Debug.DrawRay(transform.TransformPoint(hinge), swingPlaneVec, Color.green);

            float CosOffset = Vector3.Dot(BldVec,swingPlaneVec);
        
            BladeI += CosOffset;
            Vector3 EffectiveTorque = Vector3.ClampMagnitude(Vector3.Cross(Vector3.Normalize(BldVec),swingPlaneVec) * (BladePID[0] * CosOffset + BladePID[1] * BladeI + Vector3.Dot(blade.GetComponent<Rigidbody>().velocity, swingPlaneVec) * BladePID[2]) +  swingPlaneVec *BladePower, HOMBMaxForce[3]);
            blade.GetComponent<Rigidbody>().ApplyEffectiveTorqueAtRelativePosition(EffectiveTorque,new Vector3(0f,-0.68f,0f));
            body.ApplyEffectiveTorqueAtRelativePosition(-1f * EffectiveTorque, new Vector3(0, 0.52f, 0));
            blade.GetComponent<Rigidbody>().AddRelativeTorque(0, Vector3.Dot(swingPlaneVec, blade.transform.forward) * 0.10f, 0);
            //Debug.DrawRay(transform.TransformPoint(hinge), EffectiveTorque, Color.white);

            //print(Vector3.Dot(BldVec, restDir));
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
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "PlayerWeapon")
        {
            //float CutDepth = collider.
            
        }
    }

}
