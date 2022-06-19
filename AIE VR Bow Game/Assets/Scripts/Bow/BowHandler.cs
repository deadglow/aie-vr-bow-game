using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowHandler : MonoBehaviour
{
	public bool drawDebug = false;
	public ProjectileType defaultProjectileType = ProjectileType.None;

	[Header("Hand Setup")]
    public Transform leftHandTransform;
	public Transform rightHandTransform;
	public bool wieldWithLeftHand = true;
	public bool aimOnlyWithBowHand = false;

	[Header("Draw Properties")]
	public Vector3 drawInteractionOffset = Vector3.zero;
	public float drawInteractionRadius = 0.2f;

	[field: SerializeField, Range(0.0f, 1.0f)]
	public float CurrentDrawPercent { get; private set; }
	[field: SerializeField]
	public bool IsDrawing { get; private set; }
	public Vector3 BowForward { get; private set; }
	public Vector3 BowUp {get; private set; }
	[Space]
	public float drawLength = 1.0f;
	public float minDrawDistance = 0.1f;

	[Header("Arrow State")]
	public ProjectileManager projectileManager;
	public Vector3 arrowFireOffset;
	public ProjectileType currentArrowType = ProjectileType.None;
	public float arrowMinDrawPercent = 0.05f;

	void Start()
	{
		if (!projectileManager)
			projectileManager = GameObject.FindGameObjectWithTag("Projectile Manager").GetComponent<ProjectileManager>();
	}

	void Update()
	{
		if (IsDrawing)
		{
			Vector3 aimVector = GetBowHand().position - GetArrowHand().position;
			float distance = aimVector.magnitude;

			if (distance == 0 || aimOnlyWithBowHand)
				BowForward = GetBowHand().forward;
			else
				BowForward = aimVector / distance;

			CurrentDrawPercent = Mathf.Clamp01((distance - minDrawDistance) / (drawLength - minDrawDistance));
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

	[ContextMenu("Try Draw")]
	public void TryDraw() => TryDraw(defaultProjectileType);
	public void TryDraw(ProjectileType arrowType)
	{
		if (IsDrawing)
			ReleaseDraw();

		Vector3 drawInteractionPoint = GetBowHand().TransformPoint(drawInteractionOffset);
		if (Vector3.SqrMagnitude(drawInteractionPoint - GetArrowHand().position) < drawInteractionRadius * drawInteractionRadius)
		{
			IsDrawing = true;
			currentArrowType = arrowType;
		}
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
