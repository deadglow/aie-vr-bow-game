using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowPathVisual : MonoBehaviour
{
    public bool showPath = true;
	public BowHandler bow;
	public LineRenderer lineRenderer;
	public float simplifyTolerance = 0.1f;
	public float iterationTimestep = 1.0f / 30.0f;
	public int maxIterations = 100;

	private ProjectileTypeLookup typeLookup;

	void Start()
	{
		typeLookup = FindObjectOfType<ProjectileTypeLookup>();
	}

	void LateUpdate()
	{
		List<Vector3> positions = new List<Vector3>();

		if (bow.currentArrowType != ProjectileType.None)
		{
			// simulate
			ProjectileData data = typeLookup.GetData(bow.currentArrowType);
			Vector3 position = bow.GetFirePoint();
			Vector3 velocity = bow.BowForward * data.GetFireSpeed(bow.CurrentDrawPercent); 
			
			bool simulate = true;
			int iterations = 0;			
			Vector3 translation;
			ProjectileCollision collision = new ProjectileCollision();
			collision.projectile = null;

			if (bow.currentArrowType == ProjectileType.TeleportArrow)
			{
				int bounces = 0;
				ProjectileDataTeleport teleData = (ProjectileDataTeleport)data;
				while (simulate)
				{
					// Add to the line renderer list
					positions.Add(position);

					// Amount that the projectile has to move in this step
					translation = velocity * iterationTimestep;

					// Check for collisions along that translation
					if (Projectile.ContinuousCollisionCheck(data, position, ref translation, ref collision))
					{
						if (bounces == teleData.maxBounces)
							simulate = false;
						else
						{
							bounces++;
							// Bounce
							if (Vector3.Dot(velocity, collision.collisionNormal) < 0)
							velocity = Vector3.Reflect(velocity, collision.collisionNormal) * teleData.reboundEnergyConservation;
						}
					}
					// Apply translation
					position += translation;

					velocity += Vector3.down * data.gravity * iterationTimestep;
					iterations++;

					if (iterations > maxIterations) simulate = false;
				}	
			}
			else
			{
				while (simulate)
				{
					// Add to the line renderer list
					positions.Add(position);

					// Amount that the projectile has to move in this step
					translation = velocity * iterationTimestep;

					// Check for collisions along that translation
					if (Projectile.ContinuousCollisionCheck(data, position, ref translation, ref collision))
					{
						simulate = false;
					}
					// Apply translation
					position += translation;

					velocity += Vector3.down * data.gravity * iterationTimestep;
					iterations++;

					if (iterations > maxIterations) simulate = false;
				}
			}
		}
		
		lineRenderer.positionCount = positions.Count;
		lineRenderer.SetPositions(positions.ToArray());
		lineRenderer.Simplify(simplifyTolerance);
	}


}
