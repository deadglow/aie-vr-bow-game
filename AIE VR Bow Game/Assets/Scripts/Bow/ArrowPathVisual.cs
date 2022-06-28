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
		if (bow.currentArrowType == ProjectileType.None) return;

		List<Vector3> positions = new List<Vector3>();
		if (bow.currentArrowType == ProjectileType.Arrow)
		{
			// simulate
			ProjectileData data = typeLookup.GetData(ProjectileType.Arrow);
			Vector3 position = bow.GetFirePoint();
			Vector3 velocity = bow.BowForward * data.GetFireSpeed(bow.CurrentDrawPercent); 
			
			bool simulate = true;
			bool collided = false;
			int iterations = 0;
			
			Vector3 translation;
			ProjectileCollision collision = new ProjectileCollision();
			collision.projectile = null;

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
					collided = true;
				}
				// Apply translation
				position += translation;

				velocity += Vector3.down * data.gravity * iterationTimestep;
				iterations++;

				if (iterations > maxIterations) simulate = false;
			}

		}

		lineRenderer.positionCount = positions.Count;
		lineRenderer.SetPositions(positions.ToArray());
		lineRenderer.Simplify(simplifyTolerance);
	}
}
