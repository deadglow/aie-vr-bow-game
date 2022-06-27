using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerConfinedVisual : MonoBehaviour
{
	[Header("References")]
    public PlayerMover mover;
	new public Camera camera;
	public Transform lastValidPosVisual;

	[Header("Culling Layers")]
	public int confinedLayer;
	private LayerMask defaultCullingMask;

	[Header("Confined Background Colors")]
	public Color innerColor;
	public Color outerColor;

	void Start()
	{
		defaultCullingMask = camera.cullingMask;
		mover.OnHeadConfined.AddListener(OnPlayerConfined);
		mover.OnHeadUnconfined.AddListener(OnPlayerUnconfined);
	}

	void LateUpdate()
	{
		if (mover.isHeadColliding)
		{
			Vector3 delta = mover.GetVectorToSafePoint();
			float distanceRatio = delta.magnitude / mover.maxReturnDistanceFromSafePoint;
			camera.backgroundColor = Color.Lerp(innerColor, outerColor, distanceRatio);

			lastValidPosVisual.position = mover.GetValidPosition();
		}
	}

	void OnPlayerConfined()
	{
		camera.cullingMask = 1 << confinedLayer;
		camera.clearFlags = CameraClearFlags.SolidColor;
	}

	void OnPlayerUnconfined()
	{
		camera.cullingMask = defaultCullingMask;
		camera.clearFlags = CameraClearFlags.Skybox;
	}
}
