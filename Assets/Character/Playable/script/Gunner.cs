using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Animations;

public class Gunner : MonoBehaviour
{
	public Gun[] guns;
	[SerializeField] private Gun selectedGun;
	
	public bool CanReload => !selectedGun.magazineFull && selectedGun.ammoCount > 0;
	public bool IsDuringReload => !selectedGun.ReloadFinished;
	
	public byte LocomotionLayerIndex => selectedGun._animLayerLocomotion;
	public byte AimLayerIndex => selectedGun._animLayerAim;
	public byte ReloadLayerIndex => selectedGun._animLayerReload;
	
	public void Shoot(bool trigger)
	{
		selectedGun.TriggerPulled = trigger;
	}
	
	public void Reload()
	{
		selectedGun.StartCoroutine(selectedGun.Reload());
		//selectedGun.Equipped = !selectedGun.Equipped;
	}
	
	private void OnGunAnimationEvent(AnimationEvent animationEvent)
	{
		print ("");
	}
	
	private void Start()
	{
		
	}
	
	public void SwitchGun(int direction)
	{
		selectedGun.Equipped = false;
		selectedGun = guns[System.Array.IndexOf(guns, selectedGun) + direction > 0? 1: -1];
		selectedGun.Equipped = true;
	}

}
