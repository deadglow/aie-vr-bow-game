using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileData : ScriptableObject
{
	[Header("Physics")]
    public float gravity = 0.0f;
	public float minFireSpeed = 0.0f;
	public float fireSpeed = 10.0f;
	public float radius = 0.1f;
	public bool forwardMatchVelocity = true;

	[Header("Lifetime")]
	public float maxDistance = 1000.0f;

	[Header("Collision")]
	public LayerMask collisionLayers;
	public bool useContinuousDetection = false;

	public virtual void Fire(Projectile instance, Vector3 spawnPosition, Vector3 forward, float speedScale = 1.0f)
	{
		instance.enabled = true;
		instance.attachmentTransform = null;
		instance.position = spawnPosition;
		instance.previousPosition = spawnPosition;

		instance.forward = forward;
		instance.previousForward = forward;

		instance.travelledDistance = 0;
		
		instance.velocity = forward * Mathf.LerpUnclamped(minFireSpeed, fireSpeed, speedScale);
	}

	public virtual void Disable(Projectile instance)
	{
		instance.enabled = false;
		instance.attachmentTransform = null;
	}

	public virtual void OnCollision(Projectile instance, ProjectileCollision collision)
	{
		
	}
}
