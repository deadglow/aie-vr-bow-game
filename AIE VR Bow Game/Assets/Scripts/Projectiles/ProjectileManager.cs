using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
	public Transform visualParent;
	public List<ProjectileTypePoolPair> poolSetup;
	private Dictionary<ProjectileType, ProjectileVisualPair> pairLookup = new Dictionary<ProjectileType, ProjectileVisualPair>();
	private Dictionary<ProjectileType, ProjectilePool> projectileLookup = new Dictionary<ProjectileType, ProjectilePool>();

	void Awake()
	{
		// init dictionary
		for (int i = 0; i < poolSetup.Count; ++i)
		{
			projectileLookup.Add(poolSetup[i].type, poolSetup[i].pool);
		}
	}

	void Start()
	{
		foreach (ProjectilePool pool in projectileLookup.Values)
		{
			if (!pool.visualParent)
				pool.visualParent = visualParent;
			pool.Initialise();
		}
	}

	void FixedUpdate()
	{
		List<ProjectileCollision> collisions = new List<ProjectileCollision>();
		foreach(ProjectilePool pool in projectileLookup.Values)
		{
			foreach (Projectile projectile in pool.projectiles)
			{
				if (projectile.CanMove())
					projectile.Move(ref collisions);
			}
		}

		// Handle collisions
		for (int i = 0; i < collisions.Count; ++i)
		{
			collisions[i].projectile.OnCollision(collisions[i]);
		}
	}

	void LateUpdate()
	{
		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		float t = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);

		foreach(ProjectilePool pool in projectileLookup.Values)
		{
			foreach(ProjectileVisual visual in pool.visuals)
			{
				if (visual.gameObject.activeSelf)
					visual.UpdatePosition(t);
			}
		}
	}

	public Projectile FireProjectile(ProjectileType type, Vector3 spawnPos, Vector3 direction, float speedScale = 1.0f)
	{
		Projectile projectile = projectileLookup[type].RequestProjectile();
		projectile.Fire(spawnPos, direction, speedScale);

		return projectile;
	}


	[System.Serializable]
	public struct ProjectileTypePoolPair
	{
		[field: SerializeField]
		public ProjectileType type { get; private set; }
		[field: SerializeField]
		public ProjectilePool pool { get; private set; }
	}
}
