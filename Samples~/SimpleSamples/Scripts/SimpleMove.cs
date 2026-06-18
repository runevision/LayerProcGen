using UnityEngine;

public class SimpleMove : MonoBehaviour {
	public Vector3 velocity;

	void Update() {
		transform.position += velocity * Time.deltaTime;
	}
}
