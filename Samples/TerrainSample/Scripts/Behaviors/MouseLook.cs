using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class MouseLook {
	public float sensitivityGamepad = 8f;
	public float sensitivityMouse = 0.4f;
	public bool clampVerticalRotation = true;
	public float MinimumX = -90F;
	public float MaximumX = 90F;
	public bool smooth;
	public float smoothTime = 5f;

	const float gamepadSensitivityExp = 20;

	Vector3 lookEuler;
	Vector3 lookEulerTarget;
	bool mouseWasUnlocked;

	public void Init(Transform character, Transform camera) {
		lookEuler = new Vector3(
			Mathf.Repeat(camera.localEulerAngles.x + 180, 360) - 180,
			character.eulerAngles.y, 0);
		lookEulerTarget = lookEuler;
	}

	public void LookRotation(Transform character, Transform camera, InputAction lookAction) {
		Vector2 look = lookAction.ReadValue<Vector2>();

		InputDevice device = lookAction.activeControl?.device;
		if (device is Gamepad) {
			float mag = Mathf.Min(look.magnitude, 1f);
			look = mag == 0 ? Vector2.zero : look / mag;
			mag = (Mathf.Pow(gamepadSensitivityExp, mag) - 1) / (gamepadSensitivityExp - 1);
			look *= mag * sensitivityGamepad * Time.smoothDeltaTime;
		}
		if (device is Mouse) {
			look *= sensitivityMouse;
			if (Cursor.lockState != CursorLockMode.Locked) {
				look = Vector2.zero;
				mouseWasUnlocked = true;
			}
			else if (mouseWasUnlocked) {
				// First mouse delta after locking can be large (bug in Unity) so ignore it.
				look = Vector2.zero;
				mouseWasUnlocked = false;
			}
		}

		lookEulerTarget += new Vector3(-look.y, look.x, 0);
		if (smooth) {
			lookEuler = Vector3.Lerp(lookEuler, lookEulerTarget, smoothTime * Time.smoothDeltaTime);
		}
		else {
			lookEuler = lookEulerTarget;
		}

		if (clampVerticalRotation) {
			lookEuler.x = Mathf.Clamp(lookEuler.x, -90f, 90f);
			lookEulerTarget.x = Mathf.Clamp(lookEulerTarget.x, -90f, 90f);
		}
		character.eulerAngles = Vector3.up * lookEuler.y;
		camera.localEulerAngles = Vector3.right * lookEuler.x;
	}
}
