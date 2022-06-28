using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ProjectilePool
{
	[HideInInspector]
	public ProjectileTypeLookup typeLookup;
	[HideInInspector]
	public ProjectileType projectileType = ProjectileType.None;
	public int poolSize;
	public Transform visualParent;
	[HideInInspector]
	public List<Projectile> projectiles = new List<Projectile>();
	[HideInInspector]
	public List<ProjectileVisual> visuals = new List<ProjectileVisual>();

	[Header("Events")]
	public UnityEvent<Projectile> OnRequestProjectileEvent;
	public UnityEvent<Projectile> OnProjectileAvailableEvent;

	private Queue<Projectile> available = new Queue<Projectile>();

	public void Initialise()
	{
		for (int i = 0; i < poolSize; ++i)
			AddToPool();
	}

	public void AddToPool()
	{
		Projectile projectile = new Projectile();
		projectile.projectileData = typeLookup.GetData(projectileType);
		ProjectileVisual visual = MonoBehaviour.Instantiate(typeLookup.GetPair(projectileType).visual);
		visual.transform.SetParent(visualParent);
		visual.attachedProjectile = projectile;

		projectile.OnFireEvent += visual.OnProjectileFire;
		projectile.OnDisableEvent += visual.OnProjectileDisable;
		projectile.OnCollisionEvent += visual.OnProjectileCollide;
		projectile.OnAttachEvent += visual.OnProjectileAttach;

		// This will ensure the projectile enqueues itself
		projectile.OnDisableEvent += OnProjectileAvailable;
		projectile.OnAttachEvent += OnProjectileAvailable;

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
		
		Projectile requested = available.Dequeue();
		// The projectile is probably stuck, so disable it and then use it
		if (requested.enabled)
			requested.Disable();

		OnRequestProjectileEvent.Invoke(requested);
		
		return requested;
	}

	public void QueueProjectile(Projectile proj)
	{
		if (!available.Contains(proj))
		{
			available.Enqueue(proj);
			OnProjectileAvailableEvent.Invoke(proj);
		}
	}

	public void DisableAllProjectiles()
	{
		for(int i = 0; i < projectiles.Count; ++i)
		{
			projectiles[i].Disable();
		}
	}

	private void OnProjectileAvailable(object sender, EventArgs args)
	{
		QueueProjectile((Projectile)sender);
	}
}
