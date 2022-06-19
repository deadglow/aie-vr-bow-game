using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProjectilePool
{
	public int poolSize;
	public Transform visualParent;
	public ProjectileVisualPair projVisPair;
	[HideInInspector]
	public List<Projectile> projectiles = new List<Projectile>();
	[HideInInspector]
	public List<ProjectileVisual> visuals = new List<ProjectileVisual>();

	private Queue<Projectile> available = new Queue<Projectile>();

	public void Initialise()
	{
		for (int i = 0; i < poolSize; ++i)
			AddToPool();
	}

	public void AddToPool()
	{
		Projectile projectile = new Projectile();
		projectile.projectileData = projVisPair.projectile;
		ProjectileVisual visual = MonoBehaviour.Instantiate(projVisPair.visual);
		visual.transform.SetParent(visualParent);
		visual.attachedProjectile = projectile;

		projectile.OnFireEvent += visual.OnProjectileFire;
		projectile.OnDisableEvent += visual.OnProjectileDisable;
		projectile.OnCollisionEvent += visual.OnProjectileCollide;
		// This will ensure the projectile enqueues itself
		projectile.OnDisableEvent += OnProjectileDisable;

		projectiles.Add(projectile);
		visuals.Add(visual);

		projectile.Disable();
	}

	public Projectile RequestProjectile()
	{
		if (available.Count == 0)
		{
			MonoBehaviour.print("Ran out of projectiles!!!! Current count is " + projectiles.Count);
			AddToPool();
		}
		
		return available.Dequeue();
	}

	public void QueueProjectile(Projectile proj)
	{
		available.Enqueue(proj);
	}

	private void OnProjectileDisable(object sender, EventArgs args)
	{
		QueueProjectile((Projectile)sender);
	}
}
