using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class BulletImpact : MonoBehaviour
{
	public ImpactArray arrayOfImpacts;
	static BulletImpact instance;
	private static VisualEffect impactVFX;
	private static AudioSource impactSFX;

	private void Start()
    {
	    impactVFX = GetComponent<VisualEffect>();
	    impactSFX = GetComponent<AudioSource>();
	    instance = this;
    }

    
	public static void ImpactEffect(Vector3 point, Vector3 normal, string surface = "_concrete")
	{
		impactVFX.SetVector3("impact_point", point);
		impactVFX.SetVector3("impact_normal", normal);
		impactVFX.SetInt("impact_surface_index", instance.arrayOfImpacts[surface].index);
		impactSFX.PlayOneShot(instance.arrayOfImpacts[surface].get_sfx());
		impactVFX.Play();
	}
}

[System.Serializable]
public class ImpactArray
{
	public Impact[] impacts;
	public Impact this [string id]
	{
	get
		{
			foreach(var i in impacts)
				if(i.id == id)
					return i;
			return impacts[0];
		}
	}
}

