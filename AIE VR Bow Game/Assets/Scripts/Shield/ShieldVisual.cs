using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShieldVisual : MonoBehaviour
{
    public ShieldHandler shield;
	public Transform scaler;
	public float minScale = 0;
	public float maxScale = 1;

	public AnimationCurve scaleCurve;

	public UnityEvent<Vector3> OnBulletImpactEvent;
	public UnityEvent OnDeployEvent;
	public UnityEvent OnUndeployEvent;

	void Start()
	{
		shield.OnBulletImpactEvent.AddListener(OnBulletImpact);
		shield.OnDeployEvent.AddListener(OnDeploy);
		shield.OnUndeployEvent.AddListener(OnUndeploy);
	}

	void LateUpdate()
	{
		float t = scaleCurve.Evaluate(shield.deployState);

		transform.position = shield.bow.GetBowHand().TransformPoint(Vector3.LerpUnclamped(shield.minCollider.offset, shield.maxCollider.offset, t));
		transform.rotation = shield.bow.GetBowHand().rotation;
		scaler.localScale = Vector3.one * Mathf.LerpUnclamped(minScale, maxScale, t);

		// do shield animation transition here based on shield.deploystate
	}

	public void OnBulletImpact(Vector3 point)
	{
		OnBulletImpactEvent.Invoke(point);
	}

	public void OnDeploy()
	{
		OnDeployEvent.Invoke();
	}

	public void OnUndeploy()
	{
		OnUndeployEvent.Invoke();
	}
	
}
