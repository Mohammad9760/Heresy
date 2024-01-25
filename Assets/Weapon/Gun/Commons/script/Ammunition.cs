using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ammo Type", menuName = "Shooting/Ammo")]
public class Ammunition : ScriptableObject
{
    public string name = "Name Of The Bullet Here";
	public float power = 10f, range = 1000f;
}


public static class Ammo
{
	public static int count(Ammunition ammoType)
	{
		return PlayerPrefs.GetInt(ammoType.name, 0);
	}
	public static void add(Ammunition ammoType, int count)
	{
		PlayerPrefs.SetInt(ammoType.name, Ammo.count(ammoType) + count);
	}
	public static void subtract(Ammunition ammoType, int count)
	{
		PlayerPrefs.SetInt(ammoType.name, Ammo.count(ammoType) - count);
	}
}