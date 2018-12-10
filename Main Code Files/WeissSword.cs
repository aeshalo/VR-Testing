using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeissSword : TouchableObject {
    //things to keep track of

    public GameObject Offhand;
    public CapsuleCollider Hilt;
    public Vector3 offset;
    public Vector3 rotation;
    public Vector3 CoM;
    public bool stillPickedUp;
    public ParticleSystem system;
    public ParticleSystem bladeSystem;
    public ParticleSystem arcSystem;
    public GameObject OffPrefab;
    public float DashDuration;
    public float DashSpeed;
    public float DashCooldown;

    public GameObject GlyphOuterPrefab;
    public GameObject GlyphInnerPrefab;

    private int colour;
    //private string[] modes = {"Gravity", "Ice", "Fire", "Water", "Lightning", "Peirce" };
    private Transform head;

    private bool glyph;
    private GameObject GlyphOuter;
    private GameObject GlyphInner;
    private int glyphtime;

    private AudioSource[] hits;

    //LIGHTNING
    private AudioSource LightningSound;
    private ParticleSystem Lightning;
    private ParticleSystem.MainModule Lightningmain;
    private bool thrusting;
    private bool LiEffActive;
    private SteamVR_Controller.Device holderCont;

    //ICE
    private bool IceEffActive;
    //private ParticleSystem IceEff;
    //private ParticleSystem IceChargeEff;
    //private ParticleSystem IceChargeSnowEff;
    private bool IsTouchingGround;
    public GameObject IcePrefab;

    private bool canTurn;
    private Transform trigger;
    private bool dashing;
    private Vector3 DashDir;
    private bool CanDash = true;

    float Todecimal(int num)
    {
        return ((float)num / (float)255);
    }   
    private Color[] colours;
    private ParticleSystem.MainModule sysMain;
    private ParticleSystem.MainModule bldMain;
    //private ParticleSystem.MainModule arcMain;
    //private ParticleSystem.EmissionModule sysemit;
    // Use this for initialization
    void Start () {
        //set up references to use later
        hits = transform.Find("RapierA").GetComponents<AudioSource>();
        trigger = transform.Find("Trigger").transform;
        GetComponent<Rigidbody>().centerOfMass = CoM;
        //Lightning effect init
        LightningSound = transform.Find("LightningEffBin").GetComponent<AudioSource>();
        sysMain = system.main;
        bldMain = bladeSystem.main;
        //arcMain = arcSystem.main;
        LiEffActive = false;
        Lightning = transform.Find("LightningEffBin").GetComponent<ParticleSystem>();
        Lightningmain = transform.Find("LightningEffBin").GetComponent<ParticleSystem>().main;

        colours =  new Color[] { new Color(ToZeroOne(93), ToZeroOne(47), ToZeroOne(141) , ToZeroOne(128) ),
            new Color(ToZeroOne(104) , ToZeroOne(204) , ToZeroOne(240) , ToZeroOne(128) ),
            new Color(ToZeroOne(212) , ToZeroOne(0) , ToZeroOne(0) , ToZeroOne(128) ),
            new Color(ToZeroOne(0) , ToZeroOne(0) , ToZeroOne(248) , ToZeroOne(128) ),
            new Color(ToZeroOne(245) , ToZeroOne(252) , ToZeroOne(0) , ToZeroOne(64) ),
            new Color(ToZeroOne(220) , ToZeroOne(224) , ToZeroOne(220) , ToZeroOne(128) ) };
        sysMain.startColor = colours[colour];
        bldMain.startColor = colours[colour];

        canTurn = true;
    }
	
	// Update is called once per frame
	void Update () {
        if (holder != null)
        {

            //used when sword is initially picked up (sword is inherited from grabber)
            if (isPickedUp == true && stillPickedUp == false)
            {   //Get Picked Up
                stillPickedUp = true;
                GetComponent<Rigidbody>().isKinematic = false;
                transform.position = holder.transform.TransformPoint(offset);
                transform.rotation = holder.transform.rotation * Quaternion.Euler(rotation);
                FixedJoint handjoint = AddFixedJoint();
                handjoint.connectedBody = gameObject.GetComponent<Rigidbody>();
                gameObject.layer = 10;
                head = holder.transform.parent.Find("Camera (eye)").transform;
                holderCont = holder.GetComponent<Grabber>().Controller;
                //Now DO Offhand

                //Find the other hand
                int sibling = holder.transform.GetSiblingIndex();
                if (sibling == 0)
                {
                    Offhand = holder.transform.parent.GetChild(1).gameObject;
                }
                else if (sibling == 1)
                {
                    Offhand = holder.transform.parent.GetChild(0).gameObject;
                }
                else { print("that's not right...or left!"); }

                //actually put something there
                Offhand.GetComponent<Grabber>().slave = true;
                OffPrefab.gameObject.SetActive(true);
                Offhand.GetComponent<Grabber>().HeldObject = OffPrefab;
                OffPrefab.transform.SetParent(Offhand.transform);
                OffPrefab.transform.localPosition = new Vector3(0, 0, 0);
                OffPrefab.transform.localRotation = new Quaternion(0, 0, 0, 0);
                //replace the controller model
                Offhand.transform.Find("Model").gameObject.SetActive(false);


            }
            if (isPickedUp == true)
            {   //Do every Frame while picked up
                //change trigger to reflect controller trigger
                trigger.localEulerAngles =
                new Vector3(-90 + 18 * holder.GetComponent<Grabber>().Controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x, 0, 0);
            }
            if (isPickedUp == false && stillPickedUp == true)
            {   //Get let go
                stillPickedUp = false;
                gameObject.layer = 9;
                holder.GetComponent<FixedJoint>().connectedBody = null;
                Destroy(holder.GetComponent<FixedJoint>());
                holder = null;
                //Do Offhand
                OffPrefab.transform.SetParent(null);
                Offhand.transform.Find("Model").gameObject.SetActive(true);
                Offhand.GetComponent<Grabber>().slave = false;
                Offhand.GetComponent<Grabber>().HeldObject = null;
                Offhand = null;
                OffPrefab.gameObject.SetActive(false);
            }
            if (holder.GetComponent<Grabber>().Controller.GetHairTriggerDown() && system.isEmitting == false && colour != 5 && colour != 4 && colour != 1)
            {
                //TEMP activate placeholder ability
                //print("playin!!!!");
                system.Play();


            }
            else if (holder.GetComponent<Grabber>().Controller.GetHairTriggerUp() && system.isEmitting == true)
            {
                //TEMP stop activating placeholder when trigger no longer pushed down
                //print("stoppin!!!!");
                system.Stop();
            }

            //LIGHTNING CODE
            //This code activates the Lightning element Attack [WIP]
            if (colour == 4)
            {
                if (holder.GetComponent<Grabber>().Controller.GetHairTriggerDown() && !LiEffActive && (bladeSystem.isEmitting == false))
                {
                    //Lightning.Play();
                    bladeSystem.Play();
                    arcSystem.Play();

                }

                if (bladeSystem.isEmitting == true)
                {
                    //Debug.DrawRay(transform.position, head.forward);
                    //Debug.DrawRay(transform.position, transform.forward);

                    if (Vector3.Dot(head.forward, transform.forward) > 0.86 && thrusting == true)
                    {
                        //when the sword is thrust forward, the attack triggers
                        Lightning.Play();
                        LightningSound.Play();
                        arcSystem.Stop();
                        bladeSystem.Stop();
                        LiEffActive = true;
                        StartCoroutine(ExtentionMethods.Cooldown(3f, (x) => LiEffActive = !x));
                    }
                    //if (holder.GetComponent<Grabber>().Controller.GetHairTriggerUp())
                    //{
                    //    Lightning.Stop();
                    //    LightningSound.Stop();
                    //    bladeSystem.Stop();
                    //    arcSystem.Stop();
                    //    transform.Find("LightningEffBin").GetComponent<AudioSource>().Stop();
                    //}

                    RaycastHit hitdata;
                    Debug.DrawRay(transform.TransformPoint(0, 0, 1.4f), transform.TransformDirection(0, 0, 1));
                    if (Physics.Raycast(transform.TransformPoint(0, 0, 1.4f), transform.TransformDirection(0, 0, 1), out hitdata, 15))
                    {


                        Lightningmain.startLifetime = new ParticleSystem.MinMaxCurve(hitdata.distance * 0.05f + 0.05f);

                    }
                    else
                    {
                        Lightningmain.startLifetime = new ParticleSystem.MinMaxCurve(15 * 0.05f);
                    }
                    //print("Lightning Distance: " + (Lightningmain.startLifetime.constant));
                }
            }
            

            //ICE CODE

            if (colour == 1)
            {
                if (holder.GetComponent<Grabber>().Controller.GetHairTriggerDown() && !IceEffActive && (bladeSystem.isEmitting == false) && !IsTouchingGround)
                {
                    //attack Charging
                    bladeSystem.Play();
                    //IceChargeEff.Play();
                    //IceChargeSnowEff.Play();
                }

                if (bladeSystem.isEmitting == true)
                {
                    if(IsTouchingGround) 
                    {
                        //spawn the ice wall when the sword hits the ground
                        Vector3 IceDir = Vector3.Normalize(new Vector3(transform.forward.x, 0, transform.forward.z));
                        Vector3 IcePos = new Vector3(transform.TransformPoint(0, 0, 1.376f).x,1, transform.TransformPoint(0, 0, 1.376f).z);
                        Instantiate(IcePrefab, IcePos, Quaternion.LookRotation(IceDir, Vector3.up));
                        bladeSystem.Stop();
                        IceEffActive = true;
                        StartCoroutine(ExtentionMethods.Cooldown(5f, (x) => IceEffActive = !x));
                    }
                
                }
            }

                // PEIRCE CODE

                if (holder.GetComponent<Grabber>().Controller.GetHairTriggerDown() && (colour == 5))
            {   //TEMP activate ability PEIRCE 5
                if (glyph == false)
                {   //do once at the start
                    glyph = true;
                    //print("playing balde anim");
                    bladeSystem.Play();
                    GlyphOuter = Instantiate(GlyphOuterPrefab, new Vector3(head.position.x, holder.transform.parent.transform.position.y + (float)0.02, head.position.z), new Quaternion(0, 0, 0, 0));
                    GlyphInner = Instantiate(GlyphInnerPrefab, new Vector3(head.position.x, holder.transform.parent.transform.position.y + (float)0.02, head.position.z), new Quaternion(0, 0, 0, 0));
                    GetComponent<AudioSource>().Play();
                }

            }
            else if (holder.GetComponent<Grabber>().Controller.GetHairTriggerUp() && glyph)
            {   //do every frame
                GlyphInner.GetComponent<GlyphScript>().destroyed = true;
                GlyphOuter.GetComponent<GlyphScript>().destroyed = true;
                glyph = false;
                bladeSystem.Stop();
                //bladeSystem.Clear();
            }
            if (glyph)
            {   //every frame while glyph from abl 5 is active
                glyphtime++;
                holder.GetComponent<Grabber>().Controller.TriggerHapticPulse((ushort)Mathf.Min(glyphtime * 10, 500));
            }
            
            if (holder.GetComponent<Grabber>().Controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad) && canTurn)
            {
                if (holder.GetComponent<Grabber>().Controller.GetAxis().x <= 0 && Mathf.Abs(holder.GetComponent<Grabber>().Controller.GetAxis().y) <= Mathf.Abs(holder.GetComponent<Grabber>().Controller.GetAxis().x))
                { //when left pressed
                  //go left?
                    transform.Find("RapierB").GetComponent<WeissCylinder>().Target += 60;
                    colour++;
                    if (colour == 6) { colour = 0; }
                    //print("mode:" + modes[colour]);
                    sysMain.startColor = colours[colour];
                    bldMain.startColor = colours[colour];
                    //print("ColNum: " + colour + " ColourData: " + colours[colour]);

                }
                else if (holder.GetComponent<Grabber>().Controller.GetAxis().x >= 0 && Mathf.Abs(holder.GetComponent<Grabber>().Controller.GetAxis().y) <= Mathf.Abs(holder.GetComponent<Grabber>().Controller.GetAxis().x))
                { //when right pressed
                  //go right?
                    transform.Find("RapierB").GetComponent<WeissCylinder>().Target -= 60;
                    colour--;
                    if (colour == -1) { colour = 5; }
                    //print("mode:" + modes[colour]);
                    sysMain.startColor = colours[colour];
                    bldMain.startColor = colours[colour];
                    //print("ColNum: " + colour + " ColourData: " + colours[colour]);
                }
            }

        }
    }

    void FixedUpdate()
    {
        //caluclate the movement of the blade in various scenarious, to provide data f
        if (bladeSystem.isEmitting == true && colour == 4)
        {
            //print(Vector3.Dot(head.forward, transform.forward) + " " + holderCont.velocity);
            if (holderCont.velocity.sqrMagnitude > 4 && Vector3.Dot(holderCont.velocity.normalized,head.forward) > 0.86)
            {
                thrusting = true;
            }
            else
            {
                thrusting = false;
            }
        }
        if (holder != null && isPickedUp == true && stillPickedUp == true)
        {
            //Debug.DrawRay(Offhand.transform.position, Quaternion.FromToRotation(Vector3.forward, new Vector3(Offhand.GetComponent<Grabber>().Controller.GetAxis().x, 0, Offhand.GetComponent<Grabber>().Controller.GetAxis().y)) * Vector3.forward);
            if (Offhand.GetComponent<Grabber>().Controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad) && CanDash)
            {

                
                DashDir = Vector3.Normalize(Quaternion.FromToRotation(Vector3.forward, new Vector3(Offhand.GetComponent<Grabber>().Controller.GetAxis().x, 0, Offhand.GetComponent<Grabber>().Controller.GetAxis().y)) * Vector3.ProjectOnPlane(head.transform.forward, Vector3.up));
                dashing = true;
                CanDash = false;
                StartCoroutine(ExtentionMethods.Cooldown(DashDuration + DashCooldown, (x) => CanDash = x));
                StartCoroutine(ExtentionMethods.Cooldown(DashDuration,(x)=> dashing = !x));

            }
            if (dashing)
            {
                head.parent.position += DashDir * DashSpeed;
            }
        }
    }

    //Standalone functions begin here
    float ToZeroOne(int num)
    {
        return ((float)num / (float)255);
    }

    private FixedJoint AddFixedJoint()
    {
        FixedJoint fx = holder.AddComponent<FixedJoint>();
        fx.breakForce = 200000;
        fx.breakTorque = 200000;
        return fx;
    }


    

    private void OnCollisionEnter(Collision collision)
    {
        hits[Random.Range(0, hits.Length)].Play();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("World"))
        {
            IsTouchingGround = true;

        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("World"))
        {
            IsTouchingGround = false;

        }
    }

}
