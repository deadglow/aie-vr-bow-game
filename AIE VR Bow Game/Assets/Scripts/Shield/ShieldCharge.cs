using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShieldCharge : MonoBehaviour
{
	public int currentCharge = 0;
    public int maxCharge = 3;

	public UnityEvent<int> OnChargeEvent;
	public UnityEvent<int> OnDischargeEvent;
	public UnityEvent OnDrainEvent;

	public void AddCharge(int amount = 1)
	{
		currentCharge += amount;

		if (currentCharge > maxCharge)
			currentCharge = maxCharge;

		OnChargeEvent.Invoke(amount);
	}

	public bool UseCharge(int amount = 1)
	{
		if (amount > currentCharge) return false;

		currentCharge -= amount;

		OnDischargeEvent.Invoke(amount);
		return true;
	}

	public void DrainCharge()
	{
		currentCharge = 0;
		OnDrainEvent.Invoke();
	}
}
