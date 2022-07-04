using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShieldVisual : MonoBehaviour
{
    public ShieldHandler shield;
	public Transform scaler;

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
		transform.position = shield.bow.GetBowHand().TransformPoint(shield.currentColliderProperties.offset);
		transform.rotation = shield.bow.GetBowHand().rotation;
		scaler.localScale = shield.currentColliderProperties.size;

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
