using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class InteractorSwap : MonoBehaviour
{
	bool isInteractor = false;
	public InputActionReference leftInteractorSwapReference;
	public InputActionReference rightInteractorSwapReference;

	[Space]
	[Tooltip("Returns true when swapping to interaction controller.")]
	public UnityEvent onInteractorController;
	public UnityEvent onGameplayController;

	void Start()
	{
		leftInteractorSwapReference.action.performed += SwapController;
		rightInteractorSwapReference.action.performed += SwapController;
		UpdateController();
	}

	private void SwapController(InputAction.CallbackContext obj)
	{
		isInteractor = !isInteractor;
		UpdateController();
	}
	
	private void UpdateController()
	{
		if (isInteractor)
			onInteractorController.Invoke();
		else
			onGameplayController.Invoke();
	}
}