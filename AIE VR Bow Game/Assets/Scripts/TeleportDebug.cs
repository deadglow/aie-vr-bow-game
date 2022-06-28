using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class TeleportDebug : MonoBehaviour
{
    public GameObject teleportController;
	public InputActionReference teleportActivateReference;

	[Space]
	[Header("Teleport Events")]
	public UnityEvent onTeleportActivate;
	public UnityEvent onTeleportCancel;

	void Start()
	{
		teleportActivateReference.action.performed += TeleportModeActivate;

		teleportActivateReference.action.canceled += TeleportModeCancel;
	}

	private void TeleportModeActivate(InputAction.CallbackContext obj) => onTeleportActivate.Invoke();

	private void TeleportModeCancel(InputAction.CallbackContext obj) => Invoke("DelayTeleportation", 0.1f);

	private void DelayTeleportation() => onTeleportCancel.Invoke();
}
