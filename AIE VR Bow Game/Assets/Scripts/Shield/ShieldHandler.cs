using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(EntityStatus))]
public class ShieldHandler : MonoBehaviour
{
    public BowHandler bow;
	public ShieldCharge charge;
	new private BoxCollider collider;
	private Rigidbody rb;
	private EntityStatus entStatus;

	public bool deployed { get; private set; } = false;
	public float deploySpeed = 2.0f;
	
	public ColliderProperties minCollider;
	public ColliderProperties maxCollider;
	public ColliderProperties currentColliderProperties { get; private set; } = new ColliderProperties();

	public float activeThreshold = 0.3f;
	public float deployState { get; private set; } = 0.0f;

	public UnityEvent<Vector3> OnBulletImpactEvent;
	public UnityEvent OnDeployEvent;
	public UnityEvent OnUndeployEvent;


	void Start()
	{
		collider = GetComponent<BoxCollider>();
		rb = GetComponent<Rigidbody>();
		entStatus = GetComponent<EntityStatus>();
		rb.isKinematic = true;
		entStatus.damageMultiplier = 0.0f;
		entStatus.OnDamagePointEvent.AddListener(OnHit);

		collider.enabled = false;
	}

	void FixedUpdate()
	{
		deployState = Mathf.MoveTowards(deployState, deployed? 1.0f : 0.0f, deploySpeed * Time.fixedDeltaTime);

		currentColliderProperties = ColliderProperties.Lerp(ref minCollider, ref maxCollider, deployState);
		transform.position = bow.GetBowHand().TransformPoint(currentColliderProperties.offset);
		transform.rotation = bow.GetBowHand().rotation * Quaternion.Euler(currentColliderProperties.rotation);
		collider.size = currentColliderProperties.size;

		collider.enabled = (deployState >= activeThreshold);
	}

	public void Deploy()
	{
		if (deployed) return;
		deployed = true;
		OnDeployEvent.Invoke();
	}

	public void UnDeploy()
	{
		if (!deployed) return;
		deployed = false;
		OnUndeployEvent.Invoke();
	}

	public void OnHit(Vector3 point)
	{
		charge.AddCharge();
		OnBulletImpactEvent.Invoke(point);
	}
	

	[System.Serializable]
	public struct ColliderProperties
	{
		public Vector3 offset;
		public Vector3 rotation;
		public Vector3 size;

		public static ColliderProperties Lerp(ref ColliderProperties a, ref ColliderProperties b, float t)
		{
			ColliderProperties newProperties;
			newProperties.offset = Vector3.Lerp(a.offset, b.offset, t);
			newProperties.rotation = Vector3.Slerp(a.rotation, b.rotation, t);
			newProperties.size = Vector3.Lerp(a.size, b.size, t);
			
			return newProperties;
		}
	}
}
