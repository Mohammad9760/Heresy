using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

// after how many shots should it stop shooting
public enum FireMode
{
    Single = 1,
    Semi = 1,
    Burst = 3,
    Auto = 0
}

public class Gun : MonoBehaviour
{
	public LayerMask raycastLayers;
	private Camera camera;
	private Vector3 viewportCenter = new Vector3(0.5f, 0.5f, 0.0f);
	
	public VisualEffect effects;
	public AudioClip emptySFX, reloadSFX, equipSFX;
	private AudioSource audioSource;
	
	private Animator animator;
	public byte _animLayerLocomotion, _animLayerAim, _animLayerReload;
	private ParentConstraint parentConstraint;
	private ConstraintSource constraintSource;
	
	public GunProperties Specs;
    
	public byte magazineRounds;
	public bool magazineFull => magazineRounds >= Specs.magazineCapacity;
	[HideInInspector] public bool ReloadFinished = true;
	[SerializeField] private AnimationClip reloadClip;
	public float reloadDelay => reloadClip.length;

	private float FiringDelay => 1f / Specs.RoundPerSecond; // delay between each shot
    private float nextTimeToFire;
    private byte fireControl = 0;


	private bool triggered; // is trigger pulled this frame
	public bool TriggerPulled 
    {
        // in burst-mode trigger won't be released untill all shots are done
	    get => (triggered & ReloadFinished) | (Specs.fireMode == FireMode.Burst & fireControl <= (byte)FireMode.Burst);
        set
		{
			triggered = value;
            if(value)
                StartCoroutine(Cycling());
        }
    }

	private void Start()
	{
		animator = GetComponent<Animator>();
		audioSource = GetComponent<AudioSource>();
		camera = Camera.main;
		parentConstraint = GetComponent<ParentConstraint>();
		
		//ReloadDelay = animator.runtimeAnimatorController.animationClips.First(a => a.name == "Reload").length;
		
		// temp
		ReloadFinished = true;
		magazineRounds = Specs.magazineCapacity;
		
    }

	public virtual IEnumerator Reload()
	{
		ReloadFinished = false;

		animator.SetBool("Empty", false);
		animator.SetTrigger("Reload");
		print (animator.GetCurrentAnimatorClipInfo(0).Length);
		yield return new WaitForSeconds(reloadDelay);
		//yield return new WaitWhile(() => animator.GetCurrentAnimatorStateInfo(0).IsTag("Reload"));
		magazineRounds = Specs.magazineCapacity;
	
		ReloadFinished = true;
    }

    private IEnumerator Cycling() // chambering the rounds
	{
		while(TriggerPulled)
        {
		    if(magazineRounds <= 0) // release the trigger if clip is empty
            {
			    audioSource.clip = emptySFX;
			    audioSource.Play();
	            TriggerPulled = false;
                break;
            }
            if(Time.time >= nextTimeToFire) // shoot if chambering delay is finished
            {
                nextTimeToFire = Time.time + FiringDelay;
                magazineRounds--;
	            fireControl++;
	            animator.SetBool("Empty", magazineRounds == 0);
                Shoot();
            }
            if(fireControl == (byte)Specs.fireMode) // stop firing based on fireMode
            {
                fireControl = 0;
                break;
            }
            yield return null;
        }
    }

    public virtual void Shoot()
    {
	    Ray bullet = camera.ViewportPointToRay(viewportCenter);
	    RaycastHit hit;
	    if(Physics.Raycast(bullet, out hit, 1000, raycastLayers))
	    {
	    	effects.SetVector3("impact_point", hit.point);
	    	BulletImpact.ImpactEffect(hit.point, hit.normal);
	    }
	    else
	    {
	    	effects.SetVector3("impact_point", bullet.origin + bullet.direction * 100);
	    }
	    effects.Play();
    }
    
    
	private bool equipped;
	public bool Equipped
	{
		get => equipped;
		set
		{
			equipped = value;
			// un-equipped constraint
			constraintSource = parentConstraint.GetSource(0);
			constraintSource.weight = value? 0: 1;
			parentConstraint.SetSource(0, constraintSource);
			
			// equipped constraint
			constraintSource = parentConstraint.GetSource(1);
			constraintSource.weight = value? 1: 0;
			parentConstraint.SetSource(1, constraintSource);
			
		}
	}
}
