using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Gun Property", menuName = "Shooting/Gun")]
public class GunProperties : ScriptableObject
{

    public Ammunition ammunition;
    public byte magazineCapacity = 30;
    
    public float RoundPerSecond = 10;
	public FireMode fireMode;
    
	public float muzzleClimb = 5, spread = 0.1f;
}
