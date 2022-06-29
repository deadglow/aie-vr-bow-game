using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Projectile/Data/Enemy")]
public class ProjectileDataEnemy : ProjectileData
{
	[Header("Enemy Projectile Data")]
	public int damage = 1;

	public override void OnCollision(Projectile instance, ProjectileCollision collision)
	{
		collision.collider.attachedRigidbody?.GetComponent<EntityStatus>()?.Damage(damage);
	}
}
