using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ProjectileVisual : MonoBehaviour
{
	private bool wasFired = false;
	[HideInInspector]
    public Projectile attachedProjectile;
	public UnityEvent onProjectileFireEvent;
	public UnityEvent onProjectileAttachEvent;
	public UnityEvent onProjectileDisableEvent;
	public UnityEvent<ProjectileCollision> onProjectileCollideEvent;

	public void UpdatePosition(float t)
	{
		// Match up the visual with the attachment point
		if (attachedProjectile.attachmentTransform)
		{
			transform.position = attachedProjectile.attachmentTransform.TransformPoint(attachedProjectile.attachedPosition);
			transform.rotation = Quaternion.LookRotation(attachedProjectile.attachmentTransform.TransformDirection(attachedProjectile.attachedForward), Vector3.up);
		}
		// Interpolate the visual while the projectile moves
		else
		{
			transform.position = Vector3.Lerp(attachedProjectile.previousPosition, attachedProjectile.position, t);
			Vector3 interpolatedDirection = Vector3.Slerp(attachedProjectile.previousForward, attachedProjectile.forward, t).normalized;
			
			if (interpolatedDirection.sqrMagnitude > 0)
				transform.rotation = attachedProjectile.GetRotation();
			else
				transform.rotation = Quaternion.LookRotation(Vector3.up, Vector3.forward);
		}

		if (wasFired)
		{
			onProjectileFireEvent.Invoke();
			wasFired = false;
		}
	}

	public void OnProjectileFire(object sender, EventArgs args)
	{
		gameObject.SetActive(true);
		wasFired = true;
	}

	public void OnProjectileDisable(object sender, EventArgs args)
	{
		gameObject.SetActive(false);
		onProjectileDisableEvent.Invoke();
	}

	public void OnProjectileCollide(object sender, ProjectileCollision collision)
	{
		onProjectileCollideEvent.Invoke(collision);
	}

	public void OnProjectileAttach(object sender, EventArgs args)
	{
		onProjectileAttachEvent.Invoke();
	}
}
