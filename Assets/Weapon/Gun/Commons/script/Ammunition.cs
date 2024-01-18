using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ammo Type", menuName = "Shooting/Ammo")]
public class Ammunition : ScriptableObject
{
    public string name = "Name Of The Bullet Here";
    public float power = 10f;

    public virtual void Fire()
    {
        Debug.Log($"{name} with a power of {power} was shot");
    }
}
