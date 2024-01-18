using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Bullet Impact Data", menuName = "Shooting/Impact")]
public class Impact : ScriptableObject
{
	public string id;
	public byte index;
	public AudioClip[] audios;
	public AudioClip get_sfx() => audios[Random.Range(0, audios.Length)];
}
