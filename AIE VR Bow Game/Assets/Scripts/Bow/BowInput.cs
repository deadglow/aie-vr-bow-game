using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BowInput : MonoBehaviour
{
    public BowHandler bow;
	public ArrowHandler arrow;
	
	public InputActionReference drawBowReference;

	void Start()
	{
		drawBowReference.action.performed += TryGrab;
		drawBowReference.action.canceled += ReleaseDraw;
	}

	[ContextMenu("Try Grab")]
	void DebugTryGrab() => TryGrab(new InputAction.CallbackContext());
	void TryGrab(InputAction.CallbackContext obj)
	{
		// Try grab an arrow, but if it fails try to grab the bowstring :)
		if (!arrow.TryGrabArrow())
			bow.TryDraw(ProjectileType.None);
	}

	[ContextMenu("Release Draw")]
	void DebugReleaseDraw() => ReleaseDraw(new InputAction.CallbackContext());
	void ReleaseDraw(InputAction.CallbackContext obj)
	{
		bow.ReleaseDraw();
		arrow.ReleaseArrow();
	}
}
