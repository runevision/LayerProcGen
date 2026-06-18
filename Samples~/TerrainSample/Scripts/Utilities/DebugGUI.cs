using UnityEngine;
using UnityEngine.InputSystem;

public class DebugGUI : MonoBehaviour {
	public static bool on { get; private set; }
	
	public GameObject debugOverlay;
	
	public InputActionReference debugToggle;
	public InputActionReference screenshot;

	void OnEnable() {
		debugToggle.action.Enable();
		screenshot.action.Enable();
	}

	void OnDisable() {
		debugToggle.action.Disable();
		screenshot.action.Disable();
	}
	
	void Update() {
		// Toggle debug GUI.
		if (debugToggle.action.WasPressedThisFrame()) {
			on = !on;
			debugOverlay.SetActive(on);
			Cursor.lockState = CursorLockMode.None;
		}
		// Take screenshot.
		if (screenshot.action.WasPressedThisFrame()) {
			ScreenCapture.CaptureScreenshot(System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_Screenshot.png");
		}
	}
}
