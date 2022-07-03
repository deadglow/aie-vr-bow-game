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
	[Tooltip("The radius of the sphere that represents the players head.")]
	public float headCollisionRadius = 0.2f;
	[Tooltip("How close to the safe point you have to be for the player to return to reality.")]
	public float maxReturnDistanceFromSafePoint = 2.0f;
	public LayerMask headCollisionLayers;
	public bool isHeadColliding { get; private set; }
	private Vector3 lastValidCameraPosition;

	[Header("Falling and Ground Checks")]
	[Tooltip("Keep this at 0")]
	public float fallingSpeed = 5;
	public LayerMask groundCheckCollision;
	[Tooltip("The radius of the sphere used for ground checking. Basically the radius of the player's capsule.")]
	public float groundCheckRadius = 0.3f;
	[Tooltip("How far the ground check raycast goes")]
	public float groundCheckDistance = 50.0f;
	[Tooltip("The distance to the ground that is considered \"grounded\"")]
	public float groundedDistance = 0.05f;
	[Tooltip("How far the \"ground\" has to be for the player to be recovered.")]
	public float invalidGroundDistance = 40.0f;
	private float distanceToGround = 0;

	[Header("Safe Zone")]
	[Tooltip("Parent of all the recovery points for the player.")]
	public Transform respawnPointParent;
	private List<Transform> respawnPoints = new List<Transform>();

	[Header("Events")]
	public UnityEvent<Vector3> OnTeleportEvent;
	public UnityEvent OnTeleportFailEvent;
	public UnityEvent<Vector3> OnRespawnEvent;
	public UnityEvent OnHeadConfined;
	public UnityEvent OnHeadUnconfined;

	void Start()
	{
		lastValidCameraPosition = xrOrigin.Camera.transform.position;

		respawnPoints.Clear();
		for (int i = 0; i < respawnPointParent.childCount; ++i)
		{
			respawnPoints.Add(respawnPointParent.GetChild(i));
		}
	}

	void Update()
	{
		DoGroundCheck();
		VerifyPlayerNotUnderFloor();
		DoCollision();

		//DoGravity();


	}

	private void DoGroundCheck()
	{
		// The head pos but the y value is replaced with the floor's y value
		Vector3 playerOnFloor = xrOrigin.Camera.transform.position;
		playerOnFloor.y = xrOrigin.transform.position.y;
		Vector3 camPos = xrOrigin.Camera.transform.position;
		float distanceToFloor = Vector3.Distance(playerOnFloor, camPos);

		// Check for collision with the floor
		RaycastHit hit;
		if (Physics.SphereCast(camPos, groundCheckRadius, Vector3.down, out hit, groundCheckDistance + distanceToFloor, groundCheckCollision))
		{
			distanceToGround = hit.distance - distanceToFloor;
		}
		else
		{
			distanceToGround = groundCheckDistance;
		}
	}
	
	private void DoGravity()
	{
		if (!CanTeleport()) return;

		// Move down when the distance to ground is small enough
		if (distanceToGround > groundedDistance)
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
		// if (xrOrigin.CameraFloorOffsetObject.transform.position.y < respawnYValue)
		// {
		// 	if (respawnPoints.Count == 0)
		// 		throw new System.Exception("No respawn points found.");
			
		// 	Vector3 closestPoint = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		// 	Vector3 headPos = xrOrigin.Camera.transform.position;
		// 	for (int i = 0; i < respawnPoints.Count; ++i)
		// 	{
		// 		if ((respawnPoints[i] - headPos).sqrMagnitude < closestPoint.sqrMagnitude)
		// 			closestPoint = respawnPoints[i];
		// 	}

		// 	TeleportTo(closestPoint);
		// 	OnRespawnEvent.Invoke(closestPoint);
		// }

		// Teleport the player to the closest valid point if they have nothing under them for ages (fallen through floor)
		if (distanceToGround > invalidGroundDistance)
		{
			if (respawnPoints.Count == 0)
				throw new System.Exception("No respawn points found.");
			
			Vector3 closestPoint = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 headPos = xrOrigin.Camera.transform.position;
			for (int i = 0; i < respawnPoints.Count; ++i)
			{
				if ((respawnPoints[i].position - headPos).sqrMagnitude < closestPoint.sqrMagnitude)
					closestPoint = respawnPoints[i].position;
			}

			TeleportTo(closestPoint, true);
			OnRespawnEvent.Invoke(closestPoint);
		}
	}

	private void DoCollision()
	{
		Vector3 camPos = xrOrigin.Camera.transform.position;
		Collider[] colliders = Physics.OverlapSphere(camPos, headCollisionRadius, headCollisionLayers, QueryTriggerInteraction.Ignore);
		if (colliders.Length > 0 || distanceToGround > groundedDistance)
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
				if (Vector3.Distance(lastValidCameraPosition, camPos) < maxReturnDistanceFromSafePoint && !Physics.Linecast(lastValidCameraPosition, camPos, headCollisionLayers))
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

	public bool CanTeleport()
	{
		return allowTeleportation && !isHeadColliding;
	}

	[ContextMenu("Reset Valid Camera Position")]
	public void ResetValidCameraPosition()
	{
		lastValidCameraPosition = xrOrigin.Camera.transform.position;
	}

	public bool TeleportTo(Vector3 position, bool force = false)
	{
		if (!force && !CanTeleport()) return false;

		Vector3 headpos = position + Vector3.up * distanceToGround;
		float distanceToHead = -distanceToGround;

		// Don't teleport if your head will be in something when you teleport
		Ray ray = new Ray(position, Vector3.up);
		if (!force && Physics.Raycast(ray, distanceToHead + groundCheckRadius, headCollisionLayers) || Physics.SphereCast(ray, groundCheckRadius, distanceToHead, headCollisionLayers))
		{
			OnTeleportFailEvent.Invoke();
			return false;
		}

		// Apply the difference to the origin
		xrOrigin.MoveCameraToWorldLocation(headpos);
		ResetValidCameraPosition();

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
