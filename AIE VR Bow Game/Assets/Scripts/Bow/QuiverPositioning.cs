using UnityEngine;

public class QuiverPositioning : MonoBehaviour
{
    public Transform head;
	public bool freezeWhenLookingDown = false;
	public float freezeAngle = 45.0f;

	void Update()
	{
		transform.position = head.position;
		Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;

		if (!freezeWhenLookingDown || Vector3.Angle(head.forward, Vector3.down) > freezeAngle)
			transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
	}
}
