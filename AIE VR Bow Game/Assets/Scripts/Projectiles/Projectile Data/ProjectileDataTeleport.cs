using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Projectile/Data/Teleport")]
public class ProjectileDataTeleport : ProjectileData
{
	[Header("Teleport Data")]
	public LayerMask teleportableLayers;
    public float maxFloorAngle = 15.0f;
	public float reboundEnergyConservation = 1.0f;
	public int maxBounces = 12;

	public override void OnCollision(Projectile instance, ProjectileCollision collision)
	{
		int layerTest = teleportableLayers.value & (1 << collision.collider.gameObject.layer);

		// Teleport player if on the floor
		if (layerTest != 0 && Vector3.Angle(Vector3.up, collision.collisionNormal) < maxFloorAngle)
		{
			instance.Disable();
			TeleportPlayer(collision.collisionPoint);
			return;
		}

		instance.miscCount++;

		if (instance.miscCount > maxBounces)
		{
			instance.Disable();
			return;
		}

		// Bounce
		if (Vector3.Dot(instance.velocity, collision.collisionNormal) < 0)
			instance.velocity = Vector3.Reflect(instance.velocity, collision.collisionNormal) * reboundEnergyConservation;
	}
	
	public void TeleportPlayer(Vector3 position)
	{
		PlayerMover mover = FindObjectOfType<PlayerMover>();

		if (mover)
			mover.TeleportTo(position);
	}
}
