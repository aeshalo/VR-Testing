using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceAttackScript : MonoBehaviour {

    public float AttackTime;
    public float AttackSpeed;
    public int Damage;
    public float StunDuration;
    private bool TimeOut;
    private bool Stopped;
    private bool EffectsDone;
    private ParticleSystem[] EndEffects;
	// Use this for initialization
	void Start () {
        StartCoroutine(ExtentionMethods.Cooldown(AttackTime, (x) => TimeOut = x));
        //Debug.Log(transform.Find("FinalBlock").GetComponent<ParticleSystem>().isEmitting);
        EndEffects = new ParticleSystem[4];
        EndEffects[0] = transform.Find("FinalBlock").GetComponent<ParticleSystem>();
        EndEffects[1] = transform.Find("FinalFloor").GetComponent<ParticleSystem>();
        EndEffects[2] = transform.Find("FinalWave").GetComponent<ParticleSystem>();
        EndEffects[3] = transform.Find("FinalChill").GetComponent<ParticleSystem>();

    }
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!TimeOut)
        {
            transform.position += transform.forward * AttackSpeed;
        }
        else if(!Stopped)
        {
            Stopped = true;
            StartCoroutine(ExtentionMethods.Cooldown(10.5f, (x) => EffectsDone = x));
        }
        if (EffectsDone)
        {
            Destroy(transform.parent.gameObject);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        
        if (collider.gameObject.layer == LayerMask.NameToLayer("Damagable") && collider.tag != "Player")
        {
            TimeOut = true;
            Debug.Log(collider.name);
            
            collider.transform.parent.GetComponent<Damagable>().Health -= Damage;
            collider.transform.parent.GetComponent<Damagable>().State = ("Frozen " + StunDuration);

            transform.Find("IceBlockEff").GetComponent<ParticleSystem>().Stop();
            foreach (ParticleSystem effect in EndEffects)
            {
                effect.Play();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Collider collider = collision.collider;
        if (collider.gameObject.layer == LayerMask.NameToLayer("Damagable") && collider.tag != "Player")
        {
            TimeOut = true;
            Debug.Log(collider.name);

            collider.transform.parent.GetComponent<Damagable>().Health -= Damage;
            collider.transform.parent.GetComponent<Damagable>().State = ("Frozen " + StunDuration);

            foreach (ParticleSystem effect in EndEffects)
            {
                effect.Play();
            }
        }
    }
}
