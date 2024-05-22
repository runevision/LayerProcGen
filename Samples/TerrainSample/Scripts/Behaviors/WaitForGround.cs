using UnityEngine;

public class WaitForGround : MonoBehaviour {
	public MonoBehaviour moveBehavior;
	public bool moveOntoGround = false;
	public float distAboveGround = 1;
	
	public void Update() {
		if (moveBehavior == null)
			return;

		Vector3 pos = moveBehavior.transform.position;
		Vector3 raycastOrigin = pos;
		if (!moveBehavior.enabled)
			raycastOrigin.y = 10000;
		
		RaycastHit hit;
		if (Physics.Raycast(raycastOrigin, -Vector3.up, out hit)) {
			if (!moveBehavior.enabled) {
				if (moveOntoGround)
					moveBehavior.transform.position = hit.point + Vector3.up * distAboveGround;
				moveBehavior.enabled = true;
			}
		}
		else {
			moveBehavior.enabled = false;
		}
	}
}
