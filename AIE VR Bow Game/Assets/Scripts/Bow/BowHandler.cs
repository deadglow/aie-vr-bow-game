using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BowHandler : MonoBehaviour
{
	public bool drawDebug = false;
	public ProjectileManager projectileManager;
	private ProjectileType defaultProjectileType = ProjectileType.None;

	[Header("Hand Setup")]
    public Transform leftHandTransform;
	public Transform rightHandTransform;
	[HideInInspector]
	public bool wieldWithLeftHand = true;
	public bool aimOnlyWithBowHand = false;

	[Header("Draw Properties")]
	[Tooltip("Offset from the left hand where the draw point will be.")]
	public Vector3 drawInteractionOffset = Vector3.zero;
	[Tooltip("How close to the draw point the player's hand must be to interact with it.")]
	public float drawInteractionRadius = 0.2f;

	[field: SerializeField, Range(0.0f, 1.0f)]
	public float CurrentDrawPercent { get; private set; }
	[field: SerializeField]
	public bool IsDrawing { get; private set; }
	public Vector3 BowForward { get; private set; }
	public Vector3 BowUp {get; private set; }
	[Space]
	[Tooltip("Distance from the draw point that the bowstring can be pulled to.")]
	public float drawLength = 1.0f;

	[Header("Arrow State")]
	public Vector3 arrowFireOffset;
	[HideInInspector]
	public ProjectileType currentArrowType = ProjectileType.None;
	[Tooltip("Any lower than this and the bow will simply cancel the arrow.")]
	public float arrowMinDrawPercent = 0.05f;

	void Start()
	{
		if (!projectileManager)
			projectileManager = GameObject.FindGameObjectWithTag("Projectile Manager").GetComponent<ProjectileManager>();
	}

	void Update()
	{

		if (Keyboard.current.fKey.wasPressedThisFrame)
		{
			currentArrowType = ProjectileType.Arrow;
			FireArrow();
		}
		
		if (IsDrawing)
		{
			Vector3 aimVector = GetBowHand().position - GetArrowHand().position;
			// Offset the distance by the offset so that the measured distance is between the interaction point and the current arrow hand point
			float distance = aimVector.magnitude + drawInteractionOffset.z;

			if (distance == 0 || aimOnlyWithBowHand)
				BowForward = GetBowHand().forward;
			else
				BowForward = aimVector.normalized;

			CurrentDrawPercent = Mathf.Clamp01(distance / drawLength);
		}
		else
		{
			BowForward = GetBowHand().forward;
		}

		// set the up vector
		Vector3 lhandUpForwardComponent = Vector3.Project(GetBowHand().up, BowForward);
		BowUp = (GetBowHand().up - lhandUpForwardComponent).normalized;
	}

	
	public Transform GetBowHand()
	{
		return wieldWithLeftHand? leftHandTransform : rightHandTransform;
	}

	public Transform GetArrowHand()
	{
		return wieldWithLeftHand? rightHandTransform : leftHandTransform;
	}

	public Vector3 GetDrawPoint()
	{
		Vector3 interactionPoint = GetBowMatrix().MultiplyPoint(drawInteractionOffset);

		Vector3 maxDrawPoint = interactionPoint - BowForward * drawLength;
		return Vector3.Lerp(interactionPoint, maxDrawPoint, CurrentDrawPercent);
	}

	public Matrix4x4 GetBowMatrix()
	{
		Quaternion rotation = Quaternion.LookRotation(BowForward, BowUp);
		return Matrix4x4.TRS(GetBowHand().position, rotation, Vector3.one);
	}

	public Vector3 GetFirePoint()
	{
		Matrix4x4 matrix = GetBowMatrix();
		return matrix.MultiplyPoint(arrowFireOffset);
	}

	[ContextMenu("Try Draw")]
	public bool TryDraw() => TryDraw(defaultProjectileType);
	public bool TryDraw(ProjectileType arrowType)
	{
		if (IsDrawing)
			ReleaseDraw();

		Vector3 drawInteractionPoint = GetBowHand().TransformPoint(drawInteractionOffset);
		if (Vector3.SqrMagnitude(drawInteractionPoint - GetArrowHand().position) < drawInteractionRadius * drawInteractionRadius)
		{
			IsDrawing = true;
			currentArrowType = arrowType;
			return true;
		}

		return false;
	}

	[ContextMenu("Release Draw")]
	public void ReleaseDraw()
	{
		if (!IsDrawing) return;

		IsDrawing = false;
		
		if (currentArrowType != ProjectileType.None)
		{
			if (CurrentDrawPercent > arrowMinDrawPercent)
				FireArrow();
			else
				CancelArrow();
		}
		
		CurrentDrawPercent = 0.0f;
	}

	void FireArrow()
	{
		if (currentArrowType != ProjectileType.None)
		{
			Vector3 firePoint = GetBowHand().TransformPoint(arrowFireOffset);
			projectileManager.FireProjectile(currentArrowType, firePoint, BowForward, CurrentDrawPercent);
			currentArrowType = ProjectileType.None;
		}
	}

	[ContextMenu("Debug Fire Arrow")]
	void DebugFireArrow()
	{
		currentArrowType = ProjectileType.Arrow;
		FireArrow();
	}

	void CancelArrow()
	{
		currentArrowType = ProjectileType.None;	
	}

	void OnDrawGizmos()
	{
		if (!drawDebug) return;

		// Draw bow stuff
		Gizmos.matrix = GetBowHand().localToWorldMatrix;
		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 0.05f);

		// Draw arrow stuff
		Gizmos.matrix = GetArrowHand().localToWorldMatrix;
		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 0.05f);

		Gizmos.matrix = Matrix4x4.identity;
		Gizmos.color = Color.blue;
		Gizmos.DrawRay(GetBowHand().position, BowForward * 0.3f);
		Gizmos.color = Color.green;
		Gizmos.DrawRay(GetBowHand().position, BowUp * 0.3f);
		if (IsDrawing)
		{
			Gizmos.color = Color.Lerp(Color.green, Color.red, CurrentDrawPercent);
			Gizmos.DrawRay(GetBowHand().position, -BowForward * CurrentDrawPercent * drawLength);
		}
		else
		{
			Gizmos.color = Color.green;
			Vector3 drawPoint = GetBowHand().TransformPoint(drawInteractionOffset);
			Gizmos.DrawWireSphere(drawPoint, 0.02f);
			Gizmos.DrawWireSphere(drawPoint, drawInteractionRadius);

			Gizmos.color = Color.red;
			Vector3 firePoint = GetBowHand().TransformPoint(arrowFireOffset);
			Gizmos.DrawWireSphere(firePoint, 0.02f);
			Gizmos.DrawRay(firePoint, GetBowHand().forward * 0.1f);
		}
	}
}
