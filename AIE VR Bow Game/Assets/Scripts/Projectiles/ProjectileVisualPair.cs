using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Projectile/Projectile Visual Pair")]
public class ProjectileVisualPair : ScriptableObject
{
	[field: SerializeField]
	public ProjectileData projectile { get; private set; }
	[field: SerializeField]
	public ProjectileVisual visual { get; private set; }
}