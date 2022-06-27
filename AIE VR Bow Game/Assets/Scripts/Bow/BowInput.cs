using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BowInput : MonoBehaviour
{
    public BowHandler bow;
	public ArrowHandler arrow;
	
	public InputActionReference leftHandDrawReference;
	public InputActionReference rightHandDrawReference;

	public GameObject leftHandInteractionSphere;
	public GameObject rightHandInteractionSphere;

	void Start()
	{
		leftHandDrawReference.action.performed += TryGrabLeftHand;
		rightHandDrawReference.action.performed += TryGrabRightHand;

		leftHandDrawReference.action.canceled += ReleaseDrawLeftHand;
		rightHandDrawReference.action.canceled += ReleaseDrawRightHand;
	}

	[ContextMenu("Try Grab")]
	void DebugTryGrab() => TryGrab();
	void TryGrab()
	{
		// Try grab an arrow, but if it fails try to grab the bowstring :)
		if (!arrow.TryGrabArrow())
			bow.TryDraw(ProjectileType.None);
	}

	[ContextMenu("Release Draw")]
	void DebugReleaseDraw() => ReleaseDraw();
	void ReleaseDraw()
	{
		bow.ReleaseDraw();
		arrow.ReleaseArrow();
	}

	[ContextMenu("Swap Bow Hand")]
	public void SwapBowHand()
	{
		SetBowWieldHand(GetBowWieldHand() == WieldHand.Left? WieldHand.Right : WieldHand.Left);
	}

	public void SetBowWieldHand(WieldHand wieldHand)
	{
		bool leftHand = wieldHand == WieldHand.Left;
		leftHandInteractionSphere.SetActive(!leftHand);
		rightHandInteractionSphere.SetActive(leftHand);
		bow.wieldWithLeftHand = leftHand;
	}

	public WieldHand GetBowWieldHand()
	{
		return bow.wieldWithLeftHand? WieldHand.Left : WieldHand.Right;
	}

	void TryGrabLeftHand(InputAction.CallbackContext obj)
	{
		if (bow.wieldWithLeftHand) return;

		TryGrab();
	}

	void TryGrabRightHand(InputAction.CallbackContext obj)
	{
		if (!bow.wieldWithLeftHand) return;

		TryGrab();
	}

	void ReleaseDrawLeftHand(InputAction.CallbackContext obj)
	{
		if (bow.wieldWithLeftHand) return;

		ReleaseDraw();
	}
	
	void ReleaseDrawRightHand(InputAction.CallbackContext obj)
	{
		if (!bow.wieldWithLeftHand) return;

		ReleaseDraw();
	}

	public enum WieldHand
	{
		Left,
		Right
	}
}
