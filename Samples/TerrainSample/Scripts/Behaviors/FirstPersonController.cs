using Runevision.Common;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour {

	public InputActionReference move;
	public InputActionReference look;
	public InputActionReference jump;
	public InputActionReference runToggle;

	[SerializeField] bool m_IsWalking = true;
	[SerializeField] float m_WalkSpeed;
	[SerializeField] float m_RunSpeed;
	[SerializeField] bool m_ToggleRun;
	[SerializeField] float m_Acceleration;
	[SerializeField] float m_Breaking;
	[SerializeField] float m_JumpSpeed;
	[SerializeField] float m_MaxFallSpeed;
	[SerializeField] float m_SlopeLimit;
	[SerializeField] float m_StickToGroundForce;
	[SerializeField] float m_GravityMultiplier;
	[SerializeField] MouseLook m_MouseLook;

	Camera m_Camera;
	bool m_Jump;
	Vector2 m_Input;
	Vector3 m_MoveVelocity = Vector3.zero;
	Vector3 m_LastSmoothMove = Vector3.zero;
	CharacterController m_CharacterController;
	bool m_PreviouslyGrounded;
	bool m_Jumping;

	void Start() {
		m_CharacterController = GetComponent<CharacterController>();
		m_Camera = Camera.main;
		m_Jumping = false;
		m_MouseLook.Init(transform, m_Camera.transform);
	}

	void OnEnable() {
		move.action.Enable();
		look.action.Enable();
		jump.action.Enable();
		runToggle.action.Enable();
	}

	void OnDisable() {
		move.action.Disable();
		look.action.Disable();
		jump.action.Disable();
		runToggle.action.Disable();
	}

	void Update() {
		m_MouseLook.LookRotation(transform, m_Camera.transform, look.action);

		if (!m_Jump) {
			// Store for processing in FixedUpdate.
			m_Jump = jump.action.WasPerformedThisFrame();
		}

		if (Mouse.current.leftButton.wasPressedThisFrame && !DebugGUI.on)
			Cursor.lockState = CursorLockMode.Locked;
		if (Keyboard.current.escapeKey.wasPressedThisFrame)
			Cursor.lockState = CursorLockMode.None;

		if (!m_PreviouslyGrounded && m_CharacterController.isGrounded) {
			// Ensure vertical speed is 0 when landing on ground.
			m_MoveVelocity.y = 0f;
			m_Jumping = false;
		}
		if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded) {
			// Ensure vertical speed is 0 when starting falling.
			m_MoveVelocity.y = 0f;
		}

		m_PreviouslyGrounded = m_CharacterController.isGrounded;

		// Keep track of whether or not the character is walking or running.
		if (m_ToggleRun) {
			if (runToggle.action.WasPressedThisFrame())
				m_IsWalking = !m_IsWalking;
		}
		else {
			m_IsWalking = !runToggle.action.IsPressed();
		}
	}

	void FixedUpdate() {
		// Read input.
		float horizontal = move.action.ReadValue<Vector2>().x;
		float vertical = move.action.ReadValue<Vector2>().y;

		// Normalize input vector if it exceeds 1 in length.
		m_Input = new Vector2(horizontal, vertical);
		if (m_Input.sqrMagnitude > 1) {
			m_Input.Normalize();
		}

		// Set the desired speed to be walking or running.
		float targetSpeed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
		if (horizontal == 0 && vertical == 0)
			targetSpeed = 0;

		// World space desired move direction.
		Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;
		desiredMove *= targetSpeed;
		
		float moveDirY = m_MoveVelocity.y;
		/*m_MoveDir.x = desiredMove.x;
		m_MoveDir.z = desiredMove.z;
		m_MoveDir.y = 0;*/
		
		// Smooth movement over time.
		Vector3 smoothMove = Vector3.Lerp(m_LastSmoothMove, desiredMove, 5 * Time.deltaTime);
		float newSpeed = smoothMove.magnitude;
		if (newSpeed > 0) {
			// Restrict change in speed to specified breaking and acceleration.
			float currentSpeed = m_CharacterController.velocity.xoz().magnitude;
			smoothMove = smoothMove / newSpeed * Mathf.Clamp(
				newSpeed,
				currentSpeed - m_Breaking * Time.deltaTime,
				currentSpeed + m_Acceleration * Time.deltaTime);
		}

		m_LastSmoothMove = smoothMove;
		// Update move direction.
		m_MoveVelocity = new Vector3(smoothMove.x, m_MoveVelocity.y, smoothMove.z);

		// Handle vertical movement.
		if (m_CharacterController.isGrounded) {
			if (m_Jump) {
				m_MoveVelocity.y = m_CharacterController.velocity.y + m_JumpSpeed;
				m_Jump = false;
				m_Jumping = true;
			}
			else {
				m_MoveVelocity.y = -m_StickToGroundForce;
			}
		}
		else {
			// Apply gravity when not on ground.
			Vector3 gravity = Physics.gravity;
			if (gravity != Vector3.zero) {
				float speedAlongGravity = Vector3.Dot(m_MoveVelocity, gravity.normalized);
				float maxExtraSpeedAlongGravity = m_MaxFallSpeed - speedAlongGravity;
				Vector3 gravityEffect = gravity * m_GravityMultiplier * Time.deltaTime;
				gravityEffect = gravityEffect.Clamped(maxExtraSpeedAlongGravity);
				m_MoveVelocity += gravityEffect;
			}
		}

		m_CharacterController.Move(m_MoveVelocity * Time.deltaTime);
	}
}
