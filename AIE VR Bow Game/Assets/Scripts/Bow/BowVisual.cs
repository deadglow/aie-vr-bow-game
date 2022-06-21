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
			bowPullPoint.position = handler.GetDrawPoint();
		}
	}
}
