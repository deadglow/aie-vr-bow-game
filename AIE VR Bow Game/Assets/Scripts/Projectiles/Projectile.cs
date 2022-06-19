using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Projectile
{
	public ProjectileData projectileData;
	[Header("State")]
	public bool enabled = false;

	public Vector3 position;
	[HideInInspector]
	public Vector3 previousPosition;
	[HideInInspector]
	public Vector3 forward;
	[HideInInspector]
	public Vector3 previousForward;	
	public Vector3 velocity;
	private float travelledDistance = 0;

	[Header("Attachment")]
	public Transform attachmentTransform;
	public Vector3 attachedPosition;
	public Quaternion attachedRotation;
	
	// Events
	public event EventHandler OnFireEvent;
	public event EventHandler OnDisableEvent;
	public event EventHandler<ProjectileCollision> OnCollisionEvent;
	public event EventHandler OnAttachEvent;

	public virtual bool Move(ref List<ProjectileCollision> collisionList)
	{
		// Update the deltas
		previousPosition = position;
		previousForward = forward;
		// Set the new forward (don't if there is no velocity or velocity matching is off)
		if (projectileData.forwardMatchVelocity && velocity.sqrMagnitude > 0)
			forward = velocity.normalized;

		// Calculate translation in this step
		Vector3 translation = velocity * Time.fixedDeltaTime;
		float translationDist = translation.magnitude;
		// Figure out the direction of travel (accomodates for 0 div)
		Vector3 translationDir = (translationDist > 0)? translation / translationDist : Vector3.zero;

		// Accumulate distance travelled
		travelledDistance += translationDist;

		// Assume no collision took place
		bool collided = false;
		if (projectileData.useContinuousDetection)
		{
			// We check for collisions along the path before moving
			RaycastHit rayHit;
			if (Physics.SphereCast(position, projectileData.radius, translationDir, out rayHit, translationDist, projectileData.collisionLayers))
			{
				ProjectileCollision collision;
				collision.projectile = this;
				collision.collider = rayHit.collider;
				collision.collisionPoint = rayHit.point;
				collision.collisionNormal = rayHit.normal;
				collision.direction = translationDir;
				collision.speed = velocity.magnitude;
				collisionList.Add(collision);
				collided = true;
				// Override translation by the amount to move before hitting something
				translation = translationDir * rayHit.distance;
			}
			position += translation;
		}
		else
		{
			// Move and then check for collisions
			position += translation;

			float speed = velocity.magnitude;
			foreach(Collider col in Physics.OverlapSphere(position, projectileData.radius, projectileData.collisionLayers))
			{
				ProjectileCollision collision;
				collision.projectile = this;
				collision.collider = col;
				collision.collisionPoint = position;
				collision.collisionNormal = Vector3.zero;
				collision.direction = translationDir;
				collision.speed = speed;
				collisionList.Add(collision);
				collided = true;
			}
		}

		velocity += Vector3.down * projectileData.gravity * Time.fixedDeltaTime;
		
		//Debug.DrawLine(previousPosition, position, Color.magenta, 1.0f);
		
		// Travelled too far, disable the projectile
		if (travelledDistance > projectileData.maxDistance)
		{
			Disable();
			return false;
		}
		return collided;
	}

	public void Fire(Vector3 spawnPosition, Vector3 forward, float speedScale = 1.0f)
	{
		projectileData.Fire(this, spawnPosition, forward, speedScale);
		OnFireEvent.Invoke(this, EventArgs.Empty);
	}

	public void Disable()
	{
		projectileData.Disable(this);
		OnDisableEvent.Invoke(this, EventArgs.Empty);
	}

	public void AttachToTransform(Transform t)
	{
		attachmentTransform = t;
		attachedPosition = t.InverseTransformPoint(position);
		attachedRotation = t.rotation * GetRotation();
		OnAttachEvent.Invoke(this, EventArgs.Empty);
	}

	public void OnCollision(ProjectileCollision collision)
	{
		projectileData.OnCollision(this, collision);
		OnCollisionEvent.Invoke(this, collision);
	}

	public void ClearEvents()
	{
		OnFireEvent = null;
		OnDisableEvent = null;
		OnCollisionEvent = null;
	}

	public Projectile Clone()
	{
		Projectile clone = (Projectile)this.MemberwiseClone();
		clone.ClearEvents();

		return clone;
	}

	public bool CanMove()
	{
		if (!enabled) return false;

		if (attachmentTransform != null) return false;

		return true;
	}

	public Quaternion GetRotation()
	{
		return Quaternion.LookRotation(forward, Vector3.up);
	}

}

[System.Serializable]
public struct ProjectileCollision
{
	public Projectile projectile;
	public Vector3 collisionPoint;
	public Vector3 collisionNormal;
	public Vector3 direction;
	public float speed;
	public Collider collider;
}

public enum ProjectileType : int
{
	None,
	Arrow,
	TeleportArrow,
	EnemyProjectile
}