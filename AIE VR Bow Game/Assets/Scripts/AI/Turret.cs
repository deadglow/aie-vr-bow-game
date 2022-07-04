using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
	public float shootDelay = 2;
	private float shootTimer;
	public Transform firePoint;

	public ProjectileManager manager;
	public ProjectileType projType = ProjectileType.EnemyProjectile;
	[Range(0, 1)]
	public float fireScale = 1.0f;

	void Start()
	{
		shootTimer = shootDelay;
		manager = FindObjectOfType<ProjectileManager>();
	}

	void FixedUpdate()
	{
		shootTimer -= Time.fixedDeltaTime;

		if (shootTimer <= 0)
		{
			manager.FireProjectile(projType, firePoint.position, firePoint.forward, fireScale);
			shootTimer = shootDelay;
		}
	}
}
