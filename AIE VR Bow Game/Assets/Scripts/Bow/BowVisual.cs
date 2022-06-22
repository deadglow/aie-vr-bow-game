using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowVisual : MonoBehaviour
{
	public BowHandler handler;
	public Transform bowPullPoint;
	public int blendShapeIndex = 0;
	public SkinnedMeshRenderer bowMesh;

	public float maxDrawSpeed = 5.0f;
	public float visDrawHeldSpringStrength = 1.0f;
	public float visDrawUnheldSpringStrength = 1.0f;
	public float visDrawHeldDamperStrength = 0.5f;
	public float visDrawUnheldDamperStrength = 0.5f;
	private float visDrawVelocity = 0.0f;
	private float currentVisDrawPercent = 0.0f;

	public float minBlendShapeWeight = 0.0f;
	public float maxBlendShapeWeight = 100.0f;

    void LateUpdate()
	{
		if (handler)
		{
			transform.position = handler.leftHandTransform.position;
			transform.rotation = Quaternion.LookRotation(handler.BowForward, handler.BowUp);

			// Use a spring damper system to move between the visual bow draw and the actual bow draw
			// This allows for a visual spring effect
			currentVisDrawPercent += visDrawVelocity * Time.deltaTime;

			float lerpValue = Mathf.LerpUnclamped(minBlendShapeWeight, maxBlendShapeWeight, currentVisDrawPercent);
			bowMesh.SetBlendShapeWeight(blendShapeIndex, Mathf.Clamp(lerpValue, 0, 100));

			// The string should have a more stiff look when physically being drawn
			float springStrength = handler.IsDrawing? visDrawHeldSpringStrength : visDrawUnheldSpringStrength;
			float damperStrength = handler.IsDrawing? visDrawHeldDamperStrength : visDrawUnheldDamperStrength;
			
			float difference = handler.CurrentDrawPercent - currentVisDrawPercent;
			float springAcceleration = (difference * springStrength) - (visDrawVelocity * damperStrength);
			visDrawVelocity += springAcceleration * Time.deltaTime;
			
		}
		if (bowPullPoint)
		{
			bowPullPoint.position = handler.GetDrawPoint();
		}
	}
}
