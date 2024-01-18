using UnityEngine;

#pragma warning disable // I get warned that the virtual events in this class in never used... yeah and!?

[RequireComponent(typeof(ThirdPersonCharacter))]
public class ThirdPersonCharacterController : MonoBehaviour
{
	public ThirdPersonCharacter character;
	
    public delegate void Jump();
    public virtual event Jump onJump;
	public virtual bool strafe() => false;
	public virtual bool crouch() => false;
    public virtual bool sprint() => false;
	public virtual Vector2 move() => Vector2.zero;
    
	public void SetAnimationLayerWeight(int layerIndex, float weight)
	{
		character._animator.SetLayerWeight(layerIndex, weight);
	}
	
	private void Start() => character = GetComponent<ThirdPersonCharacter>();
}
