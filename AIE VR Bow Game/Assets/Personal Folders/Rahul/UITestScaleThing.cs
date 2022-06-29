using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITestScaleThing : MonoBehaviour
{
	public float scaleA = 0.01f;
	public float scaleB = 0.5f;
	private bool state = false;

	public void SwapScale()
	{
		transform.localScale = Vector3.one * (state? scaleA : scaleB);
		state = !state;
	}

	public void LerpScale(float t)
	{
		transform.localScale = Vector3.one * Mathf.Lerp(scaleA, scaleB, t);
	}
}
