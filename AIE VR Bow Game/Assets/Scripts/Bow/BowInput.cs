using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BowInput : MonoBehaviour
{
    public BowHandler bow;
	
	public InputActionReference drawBowReference;

	void Start()
	{
		drawBowReference.action.performed += TryDraw;
		drawBowReference.action.canceled += ReleaseDraw;
	}

	void TryDraw(InputAction.CallbackContext obj) => bow.TryDraw();

	void ReleaseDraw(InputAction.CallbackContext obj) => bow.ReleaseDraw();
}
