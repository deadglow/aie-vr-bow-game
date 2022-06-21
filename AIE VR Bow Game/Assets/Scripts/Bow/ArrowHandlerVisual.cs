using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowHandlerVisual : MonoBehaviour
{
	public ArrowHandler handler;
	public Vector3 arrowKnockOffset;
	public Vector3 arrowHandOffset;
	public ProjectileType currentType = ProjectileType.None;

	[Header("Arrow Models")]
	public List<TypeVisualPair> typeVisualPairsList;
	private Dictionary<ProjectileType, Transform> visualLookup = new Dictionary<ProjectileType, Transform>();

	void Start()
	{
		foreach (TypeVisualPair visualPair in typeVisualPairsList)
		{
			Transform instance = Instantiate<Transform>(visualPair.visualPrefab);
			instance.SetParent(transform, false);
			instance.gameObject.SetActive(false);
			visualLookup.Add(visualPair.type, instance);
		}

		handler.OnEquipArrowEvent.AddListener(EquipArrow);
	}

    // Update is called once per frame
    void LateUpdate()
    {
        if (handler.currentArrowType != ProjectileType.None)
		{
			if (handler.isKnocked)
			{
				Matrix4x4 mat = Matrix4x4.TRS(handler.bow.GetDrawPoint(), Quaternion.LookRotation(handler.bow.BowForward, handler.bow.BowUp), Vector3.one);
				transform.position = mat.MultiplyPoint(arrowKnockOffset);
				transform.rotation = Quaternion.LookRotation(handler.bow.BowForward, handler.bow.BowUp);
			}
			else
			{
				Transform arrowHand = handler.bow.GetArrowHand();
				transform.position = arrowHand.TransformPoint(arrowHandOffset);
				transform.rotation = Quaternion.LookRotation(arrowHand.forward, arrowHand.up);
			}
		}
    }

	void EquipArrow(ProjectileType type)
	{
		// Hide current arrow
		if (currentType != ProjectileType.None)
			visualLookup[currentType].gameObject.SetActive(false);
		
		currentType = type;

		// Show new arrow
		if (type != ProjectileType.None)
			visualLookup[currentType].gameObject.SetActive(true);
	}

	[System.Serializable]
	public struct TypeVisualPair
	{
		public ProjectileType type;
		public Transform visualPrefab;
	}
}
