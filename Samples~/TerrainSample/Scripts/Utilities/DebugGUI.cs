using UnityEngine;

public class DebugGUI : MonoBehaviour {
	public static bool on { get; private set; }
	
	public GameObject debugOverlay;

	void Update() {
		// Toggle debug GUI when pressing 1.
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			on = !on;
			debugOverlay.SetActive(on);
			Cursor.lockState = CursorLockMode.None;
		}
		// Take screenshot when pressing 2.
		if (Input.GetKeyDown(KeyCode.Alpha2)) {
			ScreenCapture.CaptureScreenshot(System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_Screenshot.png");
		}
	}
}
