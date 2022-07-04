using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Projectile/Data/Arrow")]
public class ProjectileDataArrow : ProjectileData
{
	[Header("Arrow Data")]
	public float impactForce = 100.0f;
	public int damage = 1;
	public override void OnCollision(Projectile instance, ProjectileCollision collision)
	{
		// deal damage and stuff here
		collision.collider.attachedRigidbody?.AddForce(collision.direction * impactForce, ForceMode.Impulse);

		collision.collider.attachedRigidbody?.GetComponent<EntityStatus>()?.DamageAtPoint(damage, collision.collisionPoint);

		Transform attachTransform = collision.collider.transform;
		instance.AttachToTransform(attachTransform);
	}
}
