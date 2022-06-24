using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.XR.CoreUtils;


public class PlayerMover : MonoBehaviour
{
	public bool drawDebug = false;
    public XROrigin xrOrigin;
	public bool allowTeleportation = true;

	[Header("Head Collision")]
	public float headCollisionRadius = 0.2f;
	public float maxReturnDistanceFromSafePoint = 2.0f;
	public LayerMask headCollisionLayers;
	public bool isHeadColliding { get; private set; }
	private Vector3 lastValidCameraPosition;

	[Header("Falling and Ground Checks")]
	public float fallingSpeed = 5;
	public LayerMask groundCheckCollision;
	public float groundCheckRadius = 0.3f;
	public float groundCheckDistance = 2.0f;
	public float maxGroundDistance = 0.05f;
	private float distanceToGround = 0;

	[Header("Safe Zone")]
	public float respawnYValue = -60.0f;
	public List<Vector3> respawnPoints;

	[Header("Events")]
	public UnityEvent<Vector3> OnTeleportEvent;
	public UnityEvent<Vector3> OnRespawnEvent;
	public UnityEvent OnHeadConfined;
	public UnityEvent OnHeadUnconfined;

	void Start()
	{
		lastValidCameraPosition = xrOrigin.Camera.transform.position;
	}

	void Update()
	{
		DoCollision();

		DoGroundCheck();

		DoGravity();

		VerifyPlayerNotUnderFloor();

	}

	private void DoGroundCheck()
	{
		// The head pos but the y value is replaced with the floor's y value
		Vector3 playerOnFloor = GetPlayerFloorPoint();
		Vector3 camPos = lastValidCameraPosition;
		float distanceToFloor = Vector3.Distance(playerOnFloor, camPos);

		// Check for collision with the floor
		RaycastHit hit;
		if (Physics.SphereCast(camPos, groundCheckRadius, Vector3.down, out hit, groundCheckDistance + distanceToFloor, groundCheckCollision))
		{
			distanceToGround = hit.distance - distanceToFloor;
		}
		else
			distanceToGround = groundCheckDistance;
	}
	
	private void DoGravity()
	{
		if (!CanTeleport()) return;

		// Move down when the distance to ground is small enough
		if (distanceToGround > maxGroundDistance)
		{
			// Apply gravity
			Vector3 targetPos = xrOrigin.transform.position;
			targetPos.y -= distanceToGround;
			xrOrigin.transform.position = Vector3.MoveTowards(xrOrigin.transform.position, targetPos, fallingSpeed * Time.deltaTime);
		}
	}

	private void VerifyPlayerNotUnderFloor()
	{
		// Respawn the player if they have fallen through the floor
		if (xrOrigin.CameraFloorOffsetObject.transform.position.y < respawnYValue)
		{
			if (respawnPoints.Count == 0)
				throw new System.Exception("No respawn points found.");
			
			Vector3 closestPoint = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 headPos = xrOrigin.Camera.transform.position;
			for (int i = 0; i < respawnPoints.Count; ++i)
			{
				if ((respawnPoints[i] - headPos).sqrMagnitude < closestPoint.sqrMagnitude)
					closestPoint = respawnPoints[i];
			}

			TeleportTo(closestPoint);
			OnRespawnEvent.Invoke(closestPoint);
		}
	}

	private void DoCollision()
	{
		Vector3 camPos = xrOrigin.Camera.transform.position;
		Collider[] colliders = Physics.OverlapSphere(camPos, headCollisionRadius, headCollisionLayers, QueryTriggerInteraction.Ignore);
		if (colliders.Length > 0)
		{
			if (!isHeadColliding)
			{
				OnHeadConfined.Invoke();
				isHeadColliding = true;
			}
		}
		else
		{
			if (isHeadColliding)
			{
				// Verify if the head move was valid. should allow people to not walk through a wall
				// Also make sure they're close enough to the last safe point
				if (Vector3.Distance(lastValidCameraPosition, camPos) < maxReturnDistanceFromSafePoint && !Physics.Linecast(camPos, lastValidCameraPosition, headCollisionLayers))
				{
					isHeadColliding = false;
					lastValidCameraPosition = camPos;
					OnHeadUnconfined.Invoke();
				}
			}
			else
				lastValidCameraPosition = camPos;
		}
	}

	public Vector3 GetValidPosition()
	{
		return lastValidCameraPosition;
	}

	public Vector3 GetPlayerFloorPoint()
	{
		Vector3 playerOnFloor = xrOrigin.Camera.transform.position;
		playerOnFloor.y = xrOrigin.CameraFloorOffsetObject.transform.position.y;

		return playerOnFloor;
	}

	public bool CanTeleport()
	{
		return allowTeleportation && !isHeadColliding;
	}

	[ContextMenu("Reset Valid Camera Position")]
	public void ResetValidCameraPosition()
	{
		lastValidCameraPosition = xrOrigin.Camera.transform.position;
	}

	public bool TeleportTo(Vector3 position)
	{
		if (!CanTeleport()) return false;

		// Get the delta between the current foot pos and the desired foot pos
		Vector3 currentFloorPos = GetPlayerFloorPoint();
		Vector3 delta = position - currentFloorPos;
		// Apply the difference to the origin
		xrOrigin.transform.position += delta;

		OnTeleportEvent.Invoke(position);
		
		return true;
	}

	public void Rotate(float angle)
	{
		xrOrigin.RotateAroundCameraUsingOriginUp(angle);
	}

	public Vector3 GetVectorToSafePoint()
	{
		return lastValidCameraPosition - xrOrigin.Camera.transform.position;
	}

	void OnDrawGizmos()
	{
		if (!drawDebug) return;
		Gizmos.color = isHeadColliding? Color.red : Color.green;
		Gizmos.matrix = xrOrigin.Camera.transform.localToWorldMatrix;
		Gizmos.DrawSphere(Vector3.zero, headCollisionRadius);

		if (isHeadColliding)
		{
			Gizmos.matrix = Matrix4x4.identity;
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(lastValidCameraPosition, headCollisionRadius);
			
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(lastValidCameraPosition, maxReturnDistanceFromSafePoint);
		}

		Gizmos.matrix = Matrix4x4.identity;
		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(xrOrigin.CameraFloorOffsetObject.transform.position, new Vector3(2, 0, 2));
	}
}
