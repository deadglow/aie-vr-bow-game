using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Projectile/Data/Arrow")]
public class ProjectileDataArrow : ProjectileData
{
	public float impactForce = 100.0f;
	public override void OnCollision(Projectile instance, ProjectileCollision collision)
	{
		// deal damage and stuff here
		collision.collider.attachedRigidbody?.AddForce(collision.direction * impactForce, ForceMode.Impulse);

		Transform attachTransform = collision.collider.transform;
		instance.AttachToTransform(attachTransform);
	}
}
