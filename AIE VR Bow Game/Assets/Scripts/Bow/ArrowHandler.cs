using System;
using UnityEngine;
using UnityEngine.Events;

public class ArrowHandler : MonoBehaviour
{
	public BowHandler bow;
    public ProjectileType currentArrowType { get; private set; } = ProjectileType.None;
	public bool isKnocked { get; private set; } = false;

	public float grabRadius = 0.05f;
	public LayerMask arrowGrabLayer;

	public UnityEvent<ProjectileType> onEquipArrowEvent;
	public UnityEvent<ProjectileType> onSuccessfulEquipArrowEvent;
	public UnityEvent onFailEquipEvent;

	void Update()
	{
		if (!isKnocked)
		{
			if (currentArrowType != ProjectileType.None)
			{
				if (bow.TryDraw(currentArrowType))
				{
					isKnocked = true;
				}
			}
		}
	}

	public void ReleaseArrow()
	{
		EquipArrow(ProjectileType.None);
		isKnocked = false;
	}

	void EquipArrow(ProjectileType type)
	{
		currentArrowType = type;
		onEquipArrowEvent.Invoke(type);
		if (type != ProjectileType.None)
			onSuccessfulEquipArrowEvent.Invoke(type);
	}

	public bool TryGrabArrow()
	{
		// Detect collision with the quiver
		Collider[] colliders = Physics.OverlapSphere(bow.GetArrowHand().position, grabRadius, arrowGrabLayer, QueryTriggerInteraction.Collide);

		ProjectileType quiverType = ProjectileType.None;

		if (colliders.Length > 0)
		{
			if (colliders[0].attachedRigidbody)
			{
				ArrowQuiver quiver = colliders[0].attachedRigidbody.GetComponent<ArrowQuiver>();
				quiverType = quiver.TryGetArrow();
			}
		}

		if (quiverType == ProjectileType.None)
		{
			onFailEquipEvent.Invoke();
			return false;
		}

		EquipArrow(quiverType);
		return true;
	}
}