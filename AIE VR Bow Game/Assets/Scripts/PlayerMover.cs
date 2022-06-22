using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;

using UnityEngine.XR.Interaction.Toolkit;

public class PlayerMover : MonoBehaviour
{
    public XROrigin xrOrigin;
	public Transform floor;
	float floorGroundedDistance = 0.05f;

	void Update()
	{
		Vector3 playerOnFloor = xrOrigin.Camera.transform.position;
		playerOnFloor.y = floor.position.y;
	}

	public void TeleportTo(Vector3 position)
	{
		xrOrigin.MoveCameraToWorldLocation(position);
	}
}
