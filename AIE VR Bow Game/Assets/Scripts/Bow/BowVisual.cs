using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowVisual : MonoBehaviour
{
	public BowHandler handler;
	public Transform bowPullPoint;

    void LateUpdate()
	{
		if (handler)
		{
			transform.position = handler.leftHandTransform.position;
			transform.rotation = Quaternion.LookRotation(handler.BowForward, handler.BowUp);
		}
		if (bowPullPoint)
		{
			Vector3 interactionPoint = handler.GetBowHand().position;
			interactionPoint += handler.BowForward * handler.drawInteractionOffset.z;
			interactionPoint += handler.BowUp * handler.drawInteractionOffset.y;
			interactionPoint += -Vector3.Cross(handler.BowForward, handler.BowUp) * handler.drawInteractionOffset.x;

			Vector3 offset = -handler.BowForward * handler.CurrentDrawPercent * handler.drawLength;
			bowPullPoint.position = interactionPoint + offset;
		}
	}
}
