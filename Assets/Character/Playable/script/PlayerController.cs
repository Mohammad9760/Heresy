using System.Collections;
using UnityEngine.Animations.Rigging;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine;
using Cinemachine;
// de-coupling the third person character from the input handeling and camera work
public class PlayerController : ThirdPersonCharacterController
{
	//private ThirdPersonCharacter character;

    #region exposed properties
    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    public LayerMask playerRaycastLayers;
    public Transform lookTarget;
    private Vector3 lookVelocity = Vector3.zero;
    public PositionConstraint cameraRigPositionConstraint;
    public Rig lookRig, aimRig;
	private Animator _animator => character._animator;
	
	private Gunner gun;
    #endregion

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private float crouchingCameraZoom = 0.5f;

    [SerializeField] private CinemachineVirtualCamera FollowCamera, AimCamera;
	private CinemachineVirtualCamera activeCamera => character.Strafing? AimCamera: FollowCamera;
    private CinemachineBasicMultiChannelPerlin followCameraNoise;
    public NoiseSettings sprintingNoiseProfile, walkNoiseProfile, idleNoiseProfile;

	private bool _ads;
    public bool ADS
    {
	    get => _ads;

        private set
	    {
		    if(value == _ads | gun.IsDuringReload)
			    return;
		    _ads = value;
            
		    AimCamera.gameObject.SetActive(ADS);
		    StartCoroutine (UpdateLayerWeights());
		    //ActivateAim(ADS);
        }
	}
    
	//public void ActivateAim(bool aiming)
	//{
		//AimCamera.gameObject.SetActive(aiming);
		//StartCoroutine (UpdateLayerWeights());
		//cameraRigPositionConstraint.weight = aiming? 1: 0;
		//aimRig.weight = aiming? 1: 0;
		//lookRig.weight = aiming? 0: 1;
		//_animator.SetLayerWeight(gun.LocomotionLayerIndex, aiming? 0: 1);
		//_animator.SetLayerWeight(gun.AimLayerIndex, aiming? 1: 0);
		//_animator.SetLayerWeight(gun.ReloadLayerIndex, 0);
	//}
	private IEnumerator UpdateLayerWeights(float interpolationSpeed = 5f)
	{
		_animator.SetLayerWeight(gun.ReloadLayerIndex, 0);
		
		
		var aimLayerWeight = ADS? 1f: 0f;
		for (float t = 0; ; t += Time.deltaTime * interpolationSpeed) 
		{
			cameraRigPositionConstraint.weight = aimLayerWeight * t;
			aimRig.weight = aimLayerWeight * t;
			lookRig.weight = 1f - aimLayerWeight * t;
			_animator.SetLayerWeight(gun.LocomotionLayerIndex, 1f - aimLayerWeight * t);
			_animator.SetLayerWeight(gun.AimLayerIndex, aimLayerWeight * t);
			
			yield return null;
			if(Mathf.Clamp01(t) == 1) break;
		}
	}

	private Controls.PlayerActions _input;

    public override event Jump onJump;

	public override bool strafe() => ADS; //!gun.IsDuringReload & _input.Aim.ReadValue<float>() == 1;
	
    public override bool sprint() => 0 != _input.Sprint.ReadValue<float>();

    public override Vector2 move() => _input.Move.ReadValue<Vector2>();

    public Vector2 look() => _input.Look.ReadValue<Vector2>();

    private void Awake()
    {
	    gun = GetComponent<Gunner>();
	    character = GetComponent<ThirdPersonCharacter>();
	    followCameraNoise = FollowCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
	    
	    // this property exist so that NPCs that use the thirdperson code don't move based on camera
	    character.CameraRelativeMovement = true;
        
	    // subscribing to input events
	    _input = GameManager.InputMaps.Player;
	    _input.Jump.started += (ctx) => onJump();
	    _input.Aim.performed += (ctx) => ADS = ctx.ReadValue<float>() == 1;;
	    //_input.Crouch.performed += (ctx) => crouchToggle();
	    _input.Attack.performed += (ctx) => Attack(true);
	    _input.Attack.canceled += (ctx) => Attack(false);
	    _input.Reload.performed += (ctx) => Reload();
	    _input.SwitchShoulder.performed += (ctx) => StartCoroutine(switchShoulder());

    }
    
	private void Attack(bool interaction)
	{
		if(ADS)
			gun.Shoot(interaction);
	}
	
	private void Reload()
	{
		if(!gun.CanReload) return;
		ADS = false;
		_animator.SetLayerWeight(gun.ReloadLayerIndex, 1);
		_animator.SetTrigger("Reload");
		gun.Reload();
	}

    //private IEnumerator crouchCameraDistanceAnimation()
    //{
    //    var comp = FollowCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
    //    var cur = comp.CameraDistance;
    //    var target = comp.CameraDistance + (PlayerCharacter.Crouching? -crouchingCameraZoom: crouchingCameraZoom);

    //    for(float t = 0; t < 1; t += Time.deltaTime * 3)
    //    {
    //        comp.CameraDistance = Mathf.Lerp(cur, target, t * t);
    //        yield return null;
    //    }
    //}

	private IEnumerator switchShoulder()
	{
		var side = AimCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide;
		var targetSide = 1f - side;
		var offset = AimCamera.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset;
		var targetOffset = offset * -1;
		//AimCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide = targetSide;
		//AimCamera.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = targetOffset;
		//yield return null;
		for (float t = 0; ; t += Time.deltaTime * 5.0f) 
		{
			AimCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide = Mathf.Lerp(side, targetSide, t);
			AimCamera.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = Vector3.Lerp(offset, targetOffset, t);
			//t = Mathf.Clamp01(t);
			if(Mathf.Clamp01(t) == 1) break;
			print(t);
			yield return null;
		}

	}

    private const float _threshold = 0.01f;

    private bool IsCurrentDeviceMouse
    {
        get
        {
            return true;
        }
    }

    private void Start()
    {
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        // activeCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide = 1;
    }

    private void LateUpdate()
    {
	    UpdateLooking();
	    CameraRotation();
	    UpdateCameraNoise();
	    //print (move());
    }

	private void UpdateCameraNoise() // update the cinemachine noise based on idle/walking/running
	{
		followCameraNoise.m_NoiseProfile = character.Idle | character.WallCollision? idleNoiseProfile: character.Sprint? sprintingNoiseProfile: walkNoiseProfile;
	}

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (look().sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += look().x * deltaTimeMultiplier;
            _cinemachineTargetPitch += look().y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void UpdateLooking()
	{   
		// Update Loot Target position
		
        Ray look = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;
        Vector3 targetPosition;
        if(Physics.Raycast(look,out hit, 100f, playerRaycastLayers))
        {
	        if(Vector3.Distance(hit.point, CinemachineCameraTarget.transform.position) < 1)
	        {
	        	ADS = false;
	        }
	        else
	        {
	        	ADS = character.Strafing;
	        }
	        
	        targetPosition = hit.point;
        }
        else
        {
            targetPosition = Camera.main.transform.position + Camera.main.transform.forward * 100;
        }
		if(ADS) return;
        lookTarget.position = Vector3.SmoothDamp(lookTarget.position, targetPosition, ref lookVelocity, Time.deltaTime, Vector3.Distance(lookTarget.position, targetPosition) * 20);

		// Update Look Rig wieght
        
        var lookDir = (new Vector3(lookTarget.position.x, transform.position.y, lookTarget.position.z) - transform.position).normalized;
		lookRig.weight = Utils.Remap(Vector3.Dot(lookDir, transform.forward), -1, 1, 0, 1);
    }

}
