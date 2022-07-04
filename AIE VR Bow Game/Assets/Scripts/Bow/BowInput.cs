using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BowInput : MonoBehaviour
{
    public BowHandler bow;
	public ArrowHandler arrow;
	public ShieldHandler shield;
	
	public InputActionReference leftHandDrawReference;
	public InputActionReference rightHandDrawReference;

	public InputActionReference leftHandSwapReference;
	public InputActionReference rightHandSwapReference;

	public GameObject leftHandInteractionSphere;
	public GameObject rightHandInteractionSphere;

	void Start()
	{
		leftHandDrawReference.action.performed += TryGrabLeftHand;
		rightHandDrawReference.action.performed += TryGrabRightHand;

		leftHandDrawReference.action.canceled += ReleaseDrawLeftHand;
		rightHandDrawReference.action.canceled += ReleaseDrawRightHand;

		leftHandSwapReference.action.performed += SwapBowHand;
		rightHandSwapReference.action.performed += SwapBowHand;

		SetBowWieldHand(WieldHand.Left);
	}

	[ContextMenu("Try Grab")]
	void DebugTryGrab() => TryGrab();
	void TryGrab()
	{
		if (!enabled) return;

		// Try grab an arrow, but if it fails try to grab the bowstring :)
		if (!arrow.TryGrabArrow())
			bow.TryDraw(ProjectileType.None);
	}

	[ContextMenu("Release Draw")]
	void DebugReleaseDraw() => ReleaseDraw();
	void ReleaseDraw()
	{
		if (!enabled) return;
		bow.ReleaseDraw();
		arrow.ReleaseArrow();
	}

	[ContextMenu("Swap Bow Hand")]
	public void SwapBowHand()
	{
		if (!enabled) return;

		SetBowWieldHand(GetBowWieldHand() == WieldHand.Left? WieldHand.Right : WieldHand.Left);
		bow.ReleaseDraw();
		shield.UnDeploy();
	}
	public void SwapBowHand(InputAction.CallbackContext obj) => SwapBowHand();

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
		if (bow.wieldWithLeftHand)
			shield.Deploy();
		else
			TryGrab();
	}

	void TryGrabRightHand(InputAction.CallbackContext obj)
	{
		if (!bow.wieldWithLeftHand)
			shield.Deploy();
		else
			TryGrab();
	}

	void ReleaseDrawLeftHand(InputAction.CallbackContext obj)
	{
		if (bow.wieldWithLeftHand)
			shield.UnDeploy();
		else
			ReleaseDraw();
	}
	
	void ReleaseDrawRightHand(InputAction.CallbackContext obj)
	{
		if (!bow.wieldWithLeftHand)
			shield.UnDeploy();
		else
			ReleaseDraw();
	}

	public enum WieldHand
	{
		Left,
		Right
	}
}
