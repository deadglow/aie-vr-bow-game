using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTypeLookup : MonoBehaviour
{
	public ProjectileVisualPair arrow;
	public ProjectileVisualPair teleport;
	public ProjectileVisualPair enemy;
	
	public ProjectileVisualPair GetPair(ProjectileType type)
	{
		switch(type)
		{
			case ProjectileType.Arrow:
				return arrow;

			case ProjectileType.TeleportArrow:
				return teleport;

			case ProjectileType.EnemyProjectile:
				return enemy;

			default:
				return null;
		}
	}

	public ProjectileData GetData(ProjectileType type)
	{
		return GetPair(type).projectileData;
	}
}